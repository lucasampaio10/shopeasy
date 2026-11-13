using ShopEasy.BuildingBlocks.Domain;
using ShopEasy.Payment.Application.Payments;

namespace ShopEasy.Payment.Infrastructure.Gateways;

/// <summary>
/// Stand-in for a real payment provider, with deterministic rules so every outcome of the order
/// flow can be reproduced: a token starting with "tok_declined" is refused, and so is any charge
/// above the limit.
/// </summary>
internal sealed class SimulatedPaymentGateway : IPaymentGateway
{
    public const decimal ChargeLimit = 10_000m;
    private const string DeclinedTokenPrefix = "tok_declined";

    public Task<ChargeResult> Charge(string paymentToken, Money amount, CancellationToken cancellationToken)
    {
        if (paymentToken.StartsWith(DeclinedTokenPrefix, StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult(ChargeResult.Decline("The card was declined by the issuer."));
        }

        if (amount.Amount > ChargeLimit)
        {
            return Task.FromResult(ChargeResult.Decline("The amount exceeds the card limit."));
        }

        return Task.FromResult(ChargeResult.Success($"sim_{Guid.NewGuid():N}"));
    }
}
