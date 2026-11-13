using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ShopEasy.BuildingBlocks.Infrastructure.Messaging;

namespace ShopEasy.BuildingBlocks.Infrastructure.Outbox;

/// <summary>
/// Relays pending outbox messages to the broker. Delivery is at-least-once: a crash between
/// publishing and marking the message as processed republishes it, so consumers must be idempotent.
/// </summary>
internal sealed class OutboxPublisher<TContext>(
    IServiceScopeFactory scopeFactory,
    IMessageBroker broker,
    ILogger<OutboxPublisher<TContext>> logger) : BackgroundService
    where TContext : DbContext
{
    private const int BatchSize = 20;
    private static readonly TimeSpan PollingInterval = TimeSpan.FromSeconds(1);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(PollingInterval);

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                await PublishPendingMessages(stoppingToken);
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                logger.LogWarning(exception, "Outbox polling failed; it will be retried on the next tick.");
            }
        }
    }

    private async Task PublishPendingMessages(CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<TContext>();

        var messages = await context.Set<OutboxMessage>()
            .Where(message => message.ProcessedOnUtc == null)
            .OrderBy(message => message.OccurredOnUtc)
            .Take(BatchSize)
            .ToListAsync(cancellationToken);

        if (messages.Count == 0)
        {
            return;
        }

        foreach (var message in messages)
        {
            try
            {
                broker.Publish(message.Type, Encoding.UTF8.GetBytes(message.Payload), message.Id);
                message.MarkAsProcessed(DateTime.UtcNow);
            }
            catch (Exception exception)
            {
                message.RegisterFailure(exception.Message);
                logger.LogWarning(exception, "Could not publish outbox message {MessageId} ({Type}).", message.Id, message.Type);
                break;
            }
        }

        await context.SaveChangesAsync(cancellationToken);
    }
}
