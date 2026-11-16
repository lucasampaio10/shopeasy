using ShopEasy.BuildingBlocks.Domain;

namespace ShopEasy.Payment.Domain.Payments;

public enum PaymentStatus
{
    Approved,
    Declined,
}

/// <summary>The single charge attempt made for an order. Immutable once recorded.</summary>
public sealed class Payment : AggregateRoot<Guid>
{
    private Payment()
    {
    }

    public Guid OrderId { get; private set; }

    public Guid CustomerId { get; private set; }

    public Money Amount { get; private set; } = Money.Zero();

    public PaymentStatus Status { get; private set; }

    public string? GatewayReference { get; private set; }

    public string? DeclineReason { get; private set; }

    public DateTime ProcessedAtUtc { get; private set; }

    public bool IsApproved => Status == PaymentStatus.Approved;

    public static Payment Approved(Guid orderId, Guid customerId, Money amount, string gatewayReference) =>
        Create(orderId, customerId, amount, PaymentStatus.Approved, gatewayReference, declineReason: null);

    public static Payment Declined(Guid orderId, Guid customerId, Money amount, string reason) =>
        Create(orderId, customerId, amount, PaymentStatus.Declined, gatewayReference: null, reason);

    private static Payment Create(
        Guid orderId,
        Guid customerId,
        Money amount,
        PaymentStatus status,
        string? gatewayReference,
        string? declineReason)
    {
        if (amount.Amount <= 0)
        {
            throw new DomainException("payment.invalid_amount", "A payment must be greater than zero.");
        }

        return new Payment
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            CustomerId = customerId,
            Amount = amount,
            Status = status,
            GatewayReference = gatewayReference,
            DeclineReason = declineReason,
            ProcessedAtUtc = DateTime.UtcNow,
        };
    }
}

public interface IPaymentRepository
{
    Task<bool> ExistsForOrder(Guid orderId, CancellationToken cancellationToken);

    void Add(Payment payment);
}
