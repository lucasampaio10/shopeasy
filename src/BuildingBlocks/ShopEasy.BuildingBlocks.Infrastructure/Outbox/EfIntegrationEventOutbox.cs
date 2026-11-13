using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using ShopEasy.BuildingBlocks.Infrastructure.Messaging;
using ShopEasy.BuildingBlocks.Messaging;

namespace ShopEasy.BuildingBlocks.Infrastructure.Outbox;

internal sealed class EfIntegrationEventOutbox<TContext>(TContext context) : IIntegrationEventOutbox
    where TContext : DbContext
{
    public void Add(IntegrationEvent integrationEvent)
    {
        var eventType = integrationEvent.GetType();

        context.Set<OutboxMessage>().Add(new OutboxMessage
        {
            Id = integrationEvent.EventId,
            Type = eventType.Name,
            Payload = JsonSerializer.Serialize(integrationEvent, eventType, MessageSerialization.Options),
            OccurredOnUtc = integrationEvent.OccurredOnUtc,
        });
    }
}
