using ShopEasy.BuildingBlocks.Domain;
using ShopEasy.BuildingBlocks.Messaging;
using ShopEasy.Contracts;
using ShopEasy.Ordering.Domain.Orders;

namespace ShopEasy.Ordering.Application.Orders;

/// <summary>
/// Translates what happened inside the Order aggregate into the public contract other services
/// consume. The domain model stays unaware of the broker and of who is listening.
/// </summary>
internal sealed class PublishOrderEvents(IIntegrationEventOutbox outbox) :
    IDomainEventHandler<OrderPlacedDomainEvent>,
    IDomainEventHandler<OrderPricedDomainEvent>,
    IDomainEventHandler<OrderCancelledDomainEvent>
{
    public Task Handle(OrderPlacedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        var lines = domainEvent.Items.Select(item => new OrderLineContract(item.ProductId, item.Quantity)).ToList();
        outbox.Add(new OrderPlacedIntegrationEvent(domainEvent.OrderId, lines));

        return Task.CompletedTask;
    }

    public Task Handle(OrderPricedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        outbox.Add(new OrderAwaitingPaymentIntegrationEvent(
            domainEvent.OrderId,
            domainEvent.CustomerId,
            domainEvent.Total.Amount,
            domainEvent.Total.Currency,
            domainEvent.PaymentToken));

        return Task.CompletedTask;
    }

    public Task Handle(OrderCancelledDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        outbox.Add(new OrderCancelledIntegrationEvent(domainEvent.OrderId, domainEvent.Reason));

        return Task.CompletedTask;
    }
}
