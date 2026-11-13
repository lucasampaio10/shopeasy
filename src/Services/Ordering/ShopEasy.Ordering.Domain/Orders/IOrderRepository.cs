namespace ShopEasy.Ordering.Domain.Orders;

public interface IOrderRepository
{
    Task<Order?> GetById(Guid id, CancellationToken cancellationToken);

    void Add(Order order);
}
