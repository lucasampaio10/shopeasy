using ShopEasy.BuildingBlocks.Domain;

namespace ShopEasy.Ordering.Domain.Orders;

public enum OrderStatus
{
    /// <summary>Placed and waiting for the catalog to reserve stock.</summary>
    Pending,

    /// <summary>Stock reserved and total known; waiting for the charge.</summary>
    AwaitingPayment,

    Paid,

    Cancelled,
}

public sealed record RequestedItem(Guid ProductId, int Quantity);

public sealed record ItemPrice(Guid ProductId, string ProductName, Money UnitPrice);

/// <summary>
/// Order aggregate. It owns the state machine Pending → AwaitingPayment → Paid, with Cancelled
/// reachable from any state before Paid. Transitions that already happened are ignored, so a
/// message delivered twice has no additional effect.
/// </summary>
public sealed class Order : AggregateRoot<Guid>
{
    private readonly List<OrderLine> _lines = [];

    private Order()
    {
    }

    public Guid CustomerId { get; private set; }

    public OrderStatus Status { get; private set; }

    public Address ShippingAddress { get; private set; } = null!;

    public string PaymentToken { get; private set; } = string.Empty;

    public Money Total { get; private set; } = Money.Zero();

    public DateTime PlacedAtUtc { get; private set; }

    public Guid? PaymentId { get; private set; }

    public string? CancellationReason { get; private set; }

    public IReadOnlyCollection<OrderLine> Lines => _lines.AsReadOnly();

    public static Order Place(
        Guid customerId,
        Address shippingAddress,
        string paymentToken,
        IReadOnlyCollection<RequestedItem> items)
    {
        if (customerId == Guid.Empty)
        {
            throw new DomainException("order.customer_required", "An order belongs to a customer.");
        }

        if (string.IsNullOrWhiteSpace(paymentToken))
        {
            throw new DomainException("order.payment_required", "An order needs a payment method.");
        }

        if (items.Count == 0)
        {
            throw new DomainException("order.empty", "An order needs at least one item.");
        }

        var order = new Order
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            ShippingAddress = shippingAddress,
            PaymentToken = paymentToken.Trim(),
            Status = OrderStatus.Pending,
            PlacedAtUtc = DateTime.UtcNow,
        };

        var lines = items
            .GroupBy(item => item.ProductId)
            .Select(group => new OrderLine(group.Key, group.Sum(item => item.Quantity)));
        order._lines.AddRange(lines);

        order.Raise(new OrderPlacedDomainEvent(
            order.Id,
            order._lines.Select(line => new RequestedItem(line.ProductId, line.Quantity)).ToList()));

        return order;
    }

    /// <summary>The catalog reserved every line and reported the prices it owns.</summary>
    public void ConfirmStock(IReadOnlyCollection<ItemPrice> prices)
    {
        if (Status != OrderStatus.Pending)
        {
            return;
        }

        var priceByProduct = prices.ToDictionary(price => price.ProductId);
        foreach (var line in _lines)
        {
            if (!priceByProduct.TryGetValue(line.ProductId, out var price))
            {
                throw new DomainException("order.missing_price", $"No price was reported for product {line.ProductId}.");
            }

            line.Price(price.ProductName, price.UnitPrice);
        }

        Total = _lines.Select(line => line.Total).Aggregate((sum, lineTotal) => sum.Add(lineTotal));
        Status = OrderStatus.AwaitingPayment;

        Raise(new OrderPricedDomainEvent(Id, CustomerId, Total, PaymentToken));
    }

    public void MarkAsPaid(Guid paymentId)
    {
        if (Status == OrderStatus.Paid)
        {
            return;
        }

        if (Status != OrderStatus.AwaitingPayment)
        {
            throw new DomainException("order.not_awaiting_payment", $"An order in status {Status} cannot be paid.");
        }

        PaymentId = paymentId;
        Status = OrderStatus.Paid;
    }

    public void Cancel(string reason)
    {
        if (Status == OrderStatus.Cancelled)
        {
            return;
        }

        if (Status == OrderStatus.Paid)
        {
            throw new DomainException("order.already_paid", "A paid order cannot be cancelled.");
        }

        Status = OrderStatus.Cancelled;
        CancellationReason = reason;

        Raise(new OrderCancelledDomainEvent(Id, reason));
    }
}
