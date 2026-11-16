using ShopEasy.BuildingBlocks.Cqrs;
using ShopEasy.BuildingBlocks.Results;

namespace ShopEasy.Payment.Application.Payments;

public sealed record PaymentResponse(
    Guid Id,
    Guid OrderId,
    string Status,
    decimal Amount,
    string Currency,
    string? DeclineReason,
    DateTime ProcessedAtUtc);

public interface IPaymentQueries
{
    Task<PaymentResponse?> FindByOrder(Guid orderId, CancellationToken cancellationToken);
}

public sealed record GetOrderPaymentQuery(Guid OrderId) : IQuery<PaymentResponse>;

internal sealed class GetOrderPaymentHandler(IPaymentQueries queries)
    : IQueryHandler<GetOrderPaymentQuery, PaymentResponse>
{
    public async Task<Result<PaymentResponse>> Handle(GetOrderPaymentQuery query, CancellationToken cancellationToken)
    {
        var payment = await queries.FindByOrder(query.OrderId, cancellationToken);

        return payment is null
            ? Error.NotFound("payment.not_found", $"No payment was processed for order {query.OrderId}.")
            : payment;
    }
}
