using ShopEasy.BuildingBlocks.Domain;
using ShopEasy.BuildingBlocks.Messaging;
using ShopEasy.BuildingBlocks.Persistence;
using ShopEasy.Contracts;
using ShopEasy.Payment.Domain.Payments;
using PaymentRecord = ShopEasy.Payment.Domain.Payments.Payment;

namespace ShopEasy.Payment.Application.Payments;

internal sealed class ChargeWhenOrderAwaitingPayment(
    IPaymentRepository payments,
    IPaymentGateway gateway,
    IIntegrationEventOutbox outbox,
    IUnitOfWork unitOfWork) : IIntegrationEventHandler<OrderAwaitingPaymentIntegrationEvent>
{
    public async Task Handle(OrderAwaitingPaymentIntegrationEvent order, CancellationToken cancellationToken)
    {
        // An order is charged at most once, even if the broker delivers the event again.
        if (await payments.ExistsForOrder(order.OrderId, cancellationToken))
        {
            return;
        }

        var amount = new Money(order.Amount, order.Currency);
        var charge = await gateway.Charge(order.PaymentToken, amount, cancellationToken);

        var payment = charge.Approved
            ? PaymentRecord.Approved(order.OrderId, order.CustomerId, amount, charge.Reference!)
            : PaymentRecord.Declined(order.OrderId, order.CustomerId, amount, charge.DeclineReason!);

        payments.Add(payment);
        outbox.Add(ToIntegrationEvent(payment));
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static IntegrationEvent ToIntegrationEvent(PaymentRecord payment) => payment.IsApproved
        ? new PaymentApprovedIntegrationEvent(payment.OrderId, payment.Id)
        : new PaymentDeclinedIntegrationEvent(payment.OrderId, payment.DeclineReason!);
}
