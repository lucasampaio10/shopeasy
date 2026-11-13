using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using ShopEasy.BuildingBlocks.Messaging;

namespace ShopEasy.BuildingBlocks.Infrastructure.Messaging;

internal sealed record EventSubscription(
    Type EventType,
    Func<IServiceProvider, IntegrationEvent, CancellationToken, Task> Dispatch);

/// <summary>Integration events this service listens to, keyed by event name (the routing key).</summary>
internal sealed class EventSubscriptions
{
    private readonly Dictionary<string, EventSubscription> _subscriptions = [];

    public IReadOnlyCollection<string> EventNames => _subscriptions.Keys;

    public void Add<TEvent>()
        where TEvent : IntegrationEvent
    {
        _subscriptions[typeof(TEvent).Name] = new EventSubscription(
            typeof(TEvent),
            static async (serviceProvider, integrationEvent, cancellationToken) =>
            {
                foreach (var handler in serviceProvider.GetServices<IIntegrationEventHandler<TEvent>>())
                {
                    await handler.Handle((TEvent)integrationEvent, cancellationToken);
                }
            });
    }

    public bool TryGet(string eventName, [NotNullWhen(true)] out EventSubscription? subscription) =>
        _subscriptions.TryGetValue(eventName, out subscription);
}
