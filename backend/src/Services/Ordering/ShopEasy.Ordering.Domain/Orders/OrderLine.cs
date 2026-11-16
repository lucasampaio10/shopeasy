using ShopEasy.BuildingBlocks.Domain;

namespace ShopEasy.Ordering.Domain.Orders;

public sealed class OrderLine : Entity<Guid>
{
    private OrderLine()
    {
    }

    internal OrderLine(Guid productId, int quantity)
    {
        if (quantity <= 0)
        {
            throw new DomainException("order.invalid_quantity", "Quantity must be greater than zero.");
        }

        Id = Guid.NewGuid();
        ProductId = productId;
        Quantity = quantity;
    }

    public Guid ProductId { get; private set; }

    /// <summary>Snapshot taken when the order is priced, so later catalog changes do not rewrite history.</summary>
    public string ProductName { get; private set; } = string.Empty;

    public int Quantity { get; private set; }

    public Money UnitPrice { get; private set; } = Money.Zero();

    public Money Total => UnitPrice.Multiply(Quantity);

    internal void Price(string productName, Money unitPrice)
    {
        ProductName = productName;
        UnitPrice = unitPrice;
    }
}
