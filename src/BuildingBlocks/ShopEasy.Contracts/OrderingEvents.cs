using ShopEasy.BuildingBlocks.Messaging;

namespace ShopEasy.Contracts;

public sealed record OrderLineContract(Guid ProductId, int Quantity);

/// <summary>A customer placed an order. Catalog reacts by trying to reserve stock.</summary>
public sealed record OrderPlacedIntegrationEvent(Guid OrderId, IReadOnlyList<OrderLineContract> Lines)
    : IntegrationEvent;

/// <summary>Stock is reserved and the order has a final price. Payment reacts by charging the customer.</summary>
public sealed record OrderAwaitingPaymentIntegrationEvent(
    Guid OrderId,
    Guid CustomerId,
    decimal Amount,
    string Currency,
    string PaymentToken) : IntegrationEvent;

/// <summary>The order will not be fulfilled. Catalog reacts by releasing any reserved stock.</summary>
public sealed record OrderCancelledIntegrationEvent(Guid OrderId, string Reason) : IntegrationEvent;
