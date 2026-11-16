using ShopEasy.BuildingBlocks.Domain;
using ShopEasy.BuildingBlocks.Messaging;
using ShopEasy.BuildingBlocks.Persistence;
using ShopEasy.Contracts;
using ShopEasy.Ordering.Domain.Orders;

namespace ShopEasy.Ordering.Application.Orders;

/// <summary>
/// Ordering's side of the choreographed saga. Each reaction loads the order, asks the aggregate
/// to move forward and commits; the aggregate decides whether the transition is still valid.
/// </summary>
internal sealed class OrderSaga(IOrderRepository orders, IUnitOfWork unitOfWork) :
    IIntegrationEventHandler<StockReservedIntegrationEvent>,
    IIntegrationEventHandler<StockRejectedIntegrationEvent>,
    IIntegrationEventHandler<PaymentApprovedIntegrationEvent>,
    IIntegrationEventHandler<PaymentDeclinedIntegrationEvent>
{
    public Task Handle(StockReservedIntegrationEvent stockReserved, CancellationToken cancellationToken)
    {
        var prices = stockReserved.Lines
            .Select(line => new ItemPrice(
                line.ProductId,
                line.ProductName,
                new Money(line.UnitPrice, stockReserved.Currency)))
            .ToList();

        return Advance(stockReserved.OrderId, order => order.ConfirmStock(prices), cancellationToken);
    }

    public Task Handle(StockRejectedIntegrationEvent stockRejected, CancellationToken cancellationToken) =>
        Advance(stockRejected.OrderId, order => order.Cancel(stockRejected.Reason), cancellationToken);

    public Task Handle(PaymentApprovedIntegrationEvent paymentApproved, CancellationToken cancellationToken) =>
        Advance(paymentApproved.OrderId, order => order.MarkAsPaid(paymentApproved.PaymentId), cancellationToken);

    public Task Handle(PaymentDeclinedIntegrationEvent paymentDeclined, CancellationToken cancellationToken) =>
        Advance(paymentDeclined.OrderId, order => order.Cancel(paymentDeclined.Reason), cancellationToken);

    private async Task Advance(Guid orderId, Action<Order> transition, CancellationToken cancellationToken)
    {
        var order = await orders.GetById(orderId, cancellationToken);
        if (order is null)
        {
            return;
        }

        transition(order);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
