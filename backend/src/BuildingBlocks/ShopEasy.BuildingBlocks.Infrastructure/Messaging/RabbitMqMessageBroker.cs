using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace ShopEasy.BuildingBlocks.Infrastructure.Messaging;

public interface IMessageBroker
{
    void Publish(string eventName, ReadOnlyMemory<byte> body, Guid messageId);
}

internal sealed class RabbitMqMessageBroker(RabbitMqConnection connection, IOptions<RabbitMqOptions> options)
    : IMessageBroker
{
    private static readonly TimeSpan ConfirmTimeout = TimeSpan.FromSeconds(5);
    private readonly RabbitMqOptions _options = options.Value;

    public void Publish(string eventName, ReadOnlyMemory<byte> body, Guid messageId)
    {
        using var channel = connection.CreateChannel();
        channel.ExchangeDeclare(_options.Exchange, ExchangeType.Topic, durable: true);
        channel.ConfirmSelect();

        var properties = channel.CreateBasicProperties();
        properties.Persistent = true;
        properties.MessageId = messageId.ToString();
        properties.Type = eventName;
        properties.ContentType = "application/json";

        channel.BasicPublish(_options.Exchange, routingKey: eventName, properties, body);
        channel.WaitForConfirmsOrDie(ConfirmTimeout);
    }
}
