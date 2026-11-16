using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using ShopEasy.BuildingBlocks.Messaging;

namespace ShopEasy.BuildingBlocks.Infrastructure.Messaging;

/// <summary>
/// Consumes the service queue and hands each message to its handlers inside a fresh DI scope.
/// A message that keeps failing is moved to the dead-letter queue instead of blocking the others.
/// </summary>
internal sealed class RabbitMqConsumer(
    RabbitMqConnection connection,
    EventSubscriptions subscriptions,
    IServiceScopeFactory scopeFactory,
    IOptions<RabbitMqOptions> options,
    ILogger<RabbitMqConsumer> logger) : BackgroundService
{
    private const int MaxAttempts = 3;
    private const ushort PrefetchCount = 10;
    private static readonly TimeSpan ReconnectDelay = TimeSpan.FromSeconds(3);
    private readonly RabbitMqOptions _options = options.Value;
    private IModel? _channel;

    public override void Dispose()
    {
        _channel?.Dispose();
        base.Dispose();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (subscriptions.EventNames.Count == 0)
        {
            return;
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                StartConsuming(stoppingToken);
                logger.LogInformation(
                    "Listening on queue {Queue} for {Events}.",
                    _options.Queue,
                    string.Join(", ", subscriptions.EventNames));
                return;
            }
            catch (Exception exception)
            {
                logger.LogWarning("RabbitMQ is not reachable yet ({Reason}); retrying.", exception.Message);
                await Task.Delay(ReconnectDelay, stoppingToken);
            }
        }
    }

    private void StartConsuming(CancellationToken stoppingToken)
    {
        var channel = connection.CreateChannel();
        DeclareTopology(channel);
        channel.BasicQos(prefetchSize: 0, PrefetchCount, global: false);

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.Received += (_, delivery) => OnMessageReceived(channel, delivery, stoppingToken);
        channel.BasicConsume(_options.Queue, autoAck: false, consumer);
        _channel = channel;
    }

    private void DeclareTopology(IModel channel)
    {
        channel.ExchangeDeclare(_options.Exchange, ExchangeType.Topic, durable: true);
        channel.ExchangeDeclare(_options.DeadLetterExchange, ExchangeType.Direct, durable: true);

        channel.QueueDeclare(_options.DeadLetterQueue, durable: true, exclusive: false, autoDelete: false);
        channel.QueueBind(_options.DeadLetterQueue, _options.DeadLetterExchange, routingKey: _options.Queue);

        var arguments = new Dictionary<string, object>
        {
            ["x-dead-letter-exchange"] = _options.DeadLetterExchange,
            ["x-dead-letter-routing-key"] = _options.Queue,
        };
        channel.QueueDeclare(_options.Queue, durable: true, exclusive: false, autoDelete: false, arguments);

        foreach (var eventName in subscriptions.EventNames)
        {
            channel.QueueBind(_options.Queue, _options.Exchange, routingKey: eventName);
        }
    }

    private async Task OnMessageReceived(IModel channel, BasicDeliverEventArgs delivery, CancellationToken stoppingToken)
    {
        var eventName = delivery.RoutingKey;
        if (!subscriptions.TryGet(eventName, out var subscription))
        {
            channel.BasicAck(delivery.DeliveryTag, multiple: false);
            return;
        }

        try
        {
            var integrationEvent = (IntegrationEvent)JsonSerializer.Deserialize(
                delivery.Body.Span,
                subscription.EventType,
                MessageSerialization.Options)!;

            await HandleWithRetries(subscription, integrationEvent, stoppingToken);
            channel.BasicAck(delivery.DeliveryTag, multiple: false);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Giving up on {EventName}; message sent to the dead-letter queue.", eventName);
            channel.BasicNack(delivery.DeliveryTag, multiple: false, requeue: false);
        }
    }

    private async Task HandleWithRetries(
        EventSubscription subscription,
        IntegrationEvent integrationEvent,
        CancellationToken stoppingToken)
    {
        for (var attempt = 1; ; attempt++)
        {
            try
            {
                await using var scope = scopeFactory.CreateAsyncScope();
                await subscription.Dispatch(scope.ServiceProvider, integrationEvent, stoppingToken);
                return;
            }
            catch (Exception exception) when (attempt < MaxAttempts)
            {
                logger.LogWarning(
                    exception,
                    "Attempt {Attempt} of {MaxAttempts} failed for {EventName}.",
                    attempt,
                    MaxAttempts,
                    subscription.EventType.Name);
                await Task.Delay(TimeSpan.FromMilliseconds(200 * attempt), stoppingToken);
            }
        }
    }
}
