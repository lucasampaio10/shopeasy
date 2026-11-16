using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;

namespace ShopEasy.BuildingBlocks.Domain;

internal sealed class DomainEventDispatcher(IServiceProvider serviceProvider) : IDomainEventDispatcher
{
    private static readonly ConcurrentDictionary<Type, Notifier> Notifiers = new();

    public Task Dispatch(IDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        var notifier = Notifiers.GetOrAdd(
            domainEvent.GetType(),
            static eventType => (Notifier)Activator.CreateInstance(typeof(Notifier<>).MakeGenericType(eventType))!);

        return notifier.Notify(domainEvent, serviceProvider, cancellationToken);
    }

    private abstract class Notifier
    {
        public abstract Task Notify(
            IDomainEvent domainEvent,
            IServiceProvider serviceProvider,
            CancellationToken cancellationToken);
    }

    private sealed class Notifier<TEvent> : Notifier
        where TEvent : IDomainEvent
    {
        public override async Task Notify(
            IDomainEvent domainEvent,
            IServiceProvider serviceProvider,
            CancellationToken cancellationToken)
        {
            foreach (var handler in serviceProvider.GetServices<IDomainEventHandler<TEvent>>())
            {
                await handler.Handle((TEvent)domainEvent, cancellationToken);
            }
        }
    }
}
