using ShopEasy.BuildingBlocks.Domain;

namespace ShopEasy.Ordering.Domain.Orders;

public sealed record OrderPlacedDomainEvent(Guid OrderId, IReadOnlyList<RequestedItem> Items) : DomainEvent;

public sealed record OrderPricedDomainEvent(Guid OrderId, Guid CustomerId, Money Total, string PaymentToken)
    : DomainEvent;

public sealed record OrderCancelledDomainEvent(Guid OrderId, string Reason) : DomainEvent;
