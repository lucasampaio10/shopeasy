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
        // Shown as-is in the order's cancellationReason by clients (see OrderStatusScreen in the
        // mobile app), so these two reasons are written in the storefront's language.
        if (paymentToken.StartsWith(DeclinedTokenPrefix, StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult(ChargeResult.Decline("O cartão foi recusado pela operadora."));
        }

        if (amount.Amount > ChargeLimit)
        {
            return Task.FromResult(ChargeResult.Decline("O valor excede o limite do cartão."));
        }

        return Task.FromResult(ChargeResult.Success($"sim_{Guid.NewGuid():N}"));
    }
}
