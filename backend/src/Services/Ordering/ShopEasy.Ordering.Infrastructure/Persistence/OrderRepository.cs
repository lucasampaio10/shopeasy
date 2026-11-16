using Microsoft.EntityFrameworkCore;
using ShopEasy.Ordering.Application.Orders;
using ShopEasy.Ordering.Domain.Orders;

namespace ShopEasy.Ordering.Infrastructure.Persistence;

internal sealed class OrderRepository(OrderingDbContext context) : IOrderRepository
{
    public Task<Order?> GetById(Guid id, CancellationToken cancellationToken) =>
        context.Orders.Include(order => order.Lines).FirstOrDefaultAsync(order => order.Id == id, cancellationToken);

    public void Add(Order order) => context.Orders.Add(order);
}

internal sealed class OrderQueries(OrderingDbContext context) : IOrderQueries
{
    public Task<OrderResponse?> FindOrder(Guid id, CancellationToken cancellationToken) =>
        context.Orders
            .AsNoTracking()
            .Where(order => order.Id == id)
            .Select(order => new OrderResponse(
                order.Id,
                order.CustomerId,
                order.Status.ToString(),
                order.Total.Amount,
                order.Total.Currency,
                order.PlacedAtUtc,
                order.PaymentId,
                order.CancellationReason,
                order.Lines
                    .Select(line => new OrderLineResponse(
                        line.ProductId,
                        line.ProductName,
                        line.Quantity,
                        line.UnitPrice.Amount))
                    .ToList()))
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<IReadOnlyList<OrderSummaryResponse>> ListCustomerOrders(
        Guid customerId,
        CancellationToken cancellationToken) =>
        await context.Orders
            .AsNoTracking()
            .Where(order => order.CustomerId == customerId)
            .OrderByDescending(order => order.PlacedAtUtc)
            .Select(order => new OrderSummaryResponse(
                order.Id,
                order.Status.ToString(),
                order.Total.Amount,
                order.Total.Currency,
                order.PlacedAtUtc))
            .ToListAsync(cancellationToken);
}
