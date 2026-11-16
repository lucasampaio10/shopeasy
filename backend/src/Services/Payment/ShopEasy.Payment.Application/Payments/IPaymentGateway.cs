using ShopEasy.BuildingBlocks.Domain;

namespace ShopEasy.Payment.Application.Payments;

public sealed record ChargeResult(bool Approved, string? Reference, string? DeclineReason)
{
    public static ChargeResult Success(string reference) => new(true, reference, null);

    public static ChargeResult Decline(string reason) => new(false, null, reason);
}

/// <summary>
/// Port to the external payment provider. The application only knows this contract, so swapping
/// the simulated gateway for a real one does not touch any use case.
/// </summary>
public interface IPaymentGateway
{
    Task<ChargeResult> Charge(string paymentToken, Money amount, CancellationToken cancellationToken);
}
