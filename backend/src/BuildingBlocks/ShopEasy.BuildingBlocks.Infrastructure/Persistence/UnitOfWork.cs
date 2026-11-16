using Microsoft.EntityFrameworkCore;
using ShopEasy.BuildingBlocks.Domain;
using ShopEasy.BuildingBlocks.Persistence;

namespace ShopEasy.BuildingBlocks.Infrastructure.Persistence;

/// <summary>
/// Commits the business change, the side effects of its domain events and the outbox messages
/// they produced in a single database transaction.
/// </summary>
internal sealed class UnitOfWork<TContext>(TContext context, IDomainEventDispatcher dispatcher) : IUnitOfWork
    where TContext : DbContext
{
    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        await DispatchDomainEvents(cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    private async Task DispatchDomainEvents(CancellationToken cancellationToken)
    {
        // A handler may change other aggregates and raise new events, so drain until none are left.
        while (true)
        {
            var aggregates = context.ChangeTracker.Entries<IAggregateRoot>()
                .Select(entry => entry.Entity)
                .Where(aggregate => aggregate.DomainEvents.Count > 0)
                .ToList();

            if (aggregates.Count == 0)
            {
                return;
            }

            var domainEvents = aggregates.SelectMany(aggregate => aggregate.DomainEvents).ToList();
            aggregates.ForEach(aggregate => aggregate.ClearDomainEvents());

            foreach (var domainEvent in domainEvents)
            {
                await dispatcher.Dispatch(domainEvent, cancellationToken);
            }
        }
    }
}
