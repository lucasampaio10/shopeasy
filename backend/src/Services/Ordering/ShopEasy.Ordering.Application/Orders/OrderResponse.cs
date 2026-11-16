using ShopEasy.BuildingBlocks.Results;

namespace ShopEasy.Ordering.Application.Orders;

public sealed record OrderLineResponse(Guid ProductId, string ProductName, int Quantity, decimal UnitPrice);

public sealed record OrderResponse(
    Guid Id,
    Guid CustomerId,
    string Status,
    decimal Total,
    string Currency,
    DateTime PlacedAtUtc,
    Guid? PaymentId,
    string? CancellationReason,
    IReadOnlyList<OrderLineResponse> Lines);

public sealed record OrderSummaryResponse(Guid Id, string Status, decimal Total, string Currency, DateTime PlacedAtUtc);

/// <summary>Read side of ordering: projections shaped for the screens that consume them.</summary>
public interface IOrderQueries
{
    Task<OrderResponse?> FindOrder(Guid id, CancellationToken cancellationToken);

    Task<IReadOnlyList<OrderSummaryResponse>> ListCustomerOrders(Guid customerId, CancellationToken cancellationToken);
}

internal static class OrderErrors
{
    public static Error NotFound(Guid id) => Error.NotFound("order.not_found", $"Order {id} was not found.");
}
