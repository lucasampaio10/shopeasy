using ShopEasy.BuildingBlocks.Messaging;

namespace ShopEasy.Contracts;

public sealed record PricedOrderLineContract(Guid ProductId, string ProductName, int Quantity, decimal UnitPrice);

/// <summary>Every line of the order was reserved. Carries the prices, which Catalog owns.</summary>
public sealed record StockReservedIntegrationEvent(
    Guid OrderId,
    IReadOnlyList<PricedOrderLineContract> Lines,
    string Currency) : IntegrationEvent;

/// <summary>At least one line could not be reserved, so nothing was.</summary>
public sealed record StockRejectedIntegrationEvent(Guid OrderId, string Reason) : IntegrationEvent;
