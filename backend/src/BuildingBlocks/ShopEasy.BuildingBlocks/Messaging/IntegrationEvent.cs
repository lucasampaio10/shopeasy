namespace ShopEasy.BuildingBlocks.Messaging;

/// <summary>Fact published to other services. Part of a public contract, so it only carries primitives.</summary>
public abstract record IntegrationEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();

    public DateTime OccurredOnUtc { get; init; } = DateTime.UtcNow;
}

public interface IIntegrationEventHandler<in TEvent>
    where TEvent : IntegrationEvent
{
    Task Handle(TEvent integrationEvent, CancellationToken cancellationToken);
}

/// <summary>
/// Stores an integration event in the same transaction as the business change (transactional outbox).
/// A background publisher delivers it to the broker afterwards.
/// </summary>
public interface IIntegrationEventOutbox
{
    void Add(IntegrationEvent integrationEvent);
}
