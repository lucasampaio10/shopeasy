using ShopEasy.BuildingBlocks.Cqrs;
using ShopEasy.BuildingBlocks.Results;

namespace ShopEasy.Ordering.Application.Orders;

public sealed record GetOrderQuery(Guid OrderId) : IQuery<OrderResponse>;

internal sealed class GetOrderHandler(IOrderQueries queries) : IQueryHandler<GetOrderQuery, OrderResponse>
{
    public async Task<Result<OrderResponse>> Handle(GetOrderQuery query, CancellationToken cancellationToken)
    {
        var order = await queries.FindOrder(query.OrderId, cancellationToken);

        return order is null ? OrderErrors.NotFound(query.OrderId) : order;
    }
}

public sealed record ListCustomerOrdersQuery(Guid CustomerId) : IQuery<IReadOnlyList<OrderSummaryResponse>>;

internal sealed class ListCustomerOrdersHandler(IOrderQueries queries)
    : IQueryHandler<ListCustomerOrdersQuery, IReadOnlyList<OrderSummaryResponse>>
{
    public async Task<Result<IReadOnlyList<OrderSummaryResponse>>> Handle(
        ListCustomerOrdersQuery query,
        CancellationToken cancellationToken)
    {
        var orders = await queries.ListCustomerOrders(query.CustomerId, cancellationToken);

        return Result<IReadOnlyList<OrderSummaryResponse>>.From(orders);
    }
}
