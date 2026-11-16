using NSubstitute;
using ShopEasy.BuildingBlocks.Messaging;
using ShopEasy.BuildingBlocks.Persistence;
using ShopEasy.BuildingBlocks.Results;
using ShopEasy.Contracts;
using ShopEasy.Ordering.Application.Orders;
using ShopEasy.Ordering.Domain.Orders;

namespace ShopEasy.Ordering.UnitTests;

public class OrderUseCaseTests
{
    private readonly IOrderRepository _orders = Substitute.For<IOrderRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    [Fact]
    public async Task Placing_an_order_stores_it_and_returns_its_id()
    {
        Order? stored = null;
        _orders.Add(Arg.Do<Order>(order => stored = order));
        var command = new PlaceOrderCommand(
            Guid.NewGuid(),
            new ShippingAddressInput("Av. Beira Mar, 1000", "Fortaleza", "CE", "60165-121"),
            "tok_visa",
            [new PlaceOrderItem(Guid.NewGuid(), 2)]);

        var result = await new PlaceOrderHandler(_orders, _unitOfWork).Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(stored);
        Assert.Equal(stored.Id, result.Value);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Cancelling_an_unknown_order_reports_not_found()
    {
        var result = await new CancelOrderHandler(_orders, _unitOfWork)
            .Handle(new CancelOrderCommand(Guid.NewGuid()), CancellationToken.None);

        Assert.Equal(ErrorType.NotFound, result.Error!.Type);
    }

    [Fact]
    public async Task Cancelling_a_paid_order_reports_a_conflict_and_changes_nothing()
    {
        var order = Stored(OrderMother.Paid());

        var result = await new CancelOrderHandler(_orders, _unitOfWork)
            .Handle(new CancelOrderCommand(order.Id), CancellationToken.None);

        Assert.Equal(ErrorType.Conflict, result.Error!.Type);
        Assert.Equal(OrderStatus.Paid, order.Status);
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Reserved_stock_moves_the_order_to_awaiting_payment_with_catalog_prices()
    {
        var order = Stored(OrderMother.Pending());
        var stockReserved = new StockReservedIntegrationEvent(
            order.Id,
            [
                new PricedOrderLineContract(OrderMother.Keyboard, "Teclado", 1, 400m),
                new PricedOrderLineContract(OrderMother.Mouse, "Mouse", 2, 150m),
            ],
            "BRL");

        await Saga().Handle(stockReserved, CancellationToken.None);

        Assert.Equal(OrderStatus.AwaitingPayment, order.Status);
        Assert.Equal(700m, order.Total.Amount);
    }

    [Fact]
    public async Task Rejected_stock_cancels_the_order()
    {
        var order = Stored(OrderMother.Pending());

        await Saga().Handle(new StockRejectedIntegrationEvent(order.Id, "Insufficient stock."), CancellationToken.None);

        Assert.Equal(OrderStatus.Cancelled, order.Status);
        Assert.Equal("Insufficient stock.", order.CancellationReason);
    }

    [Fact]
    public async Task A_declined_payment_cancels_the_order_so_stock_can_be_released()
    {
        var order = Stored(OrderMother.AwaitingPayment());

        await Saga().Handle(new PaymentDeclinedIntegrationEvent(order.Id, "Card declined."), CancellationToken.None);

        Assert.Equal(OrderStatus.Cancelled, order.Status);
        Assert.IsType<OrderCancelledDomainEvent>(Assert.Single(order.DomainEvents));
    }

    [Fact]
    public async Task An_approved_payment_completes_the_order()
    {
        var order = Stored(OrderMother.AwaitingPayment());
        var paymentId = Guid.NewGuid();

        await Saga().Handle(new PaymentApprovedIntegrationEvent(order.Id, paymentId), CancellationToken.None);

        Assert.Equal(OrderStatus.Paid, order.Status);
        Assert.Equal(paymentId, order.PaymentId);
    }

    [Fact]
    public async Task Domain_events_are_translated_into_the_public_contract()
    {
        var outbox = Substitute.For<IIntegrationEventOutbox>();
        var order = OrderMother.Pending();
        var placed = (OrderPlacedDomainEvent)order.DomainEvents.Single();

        await new PublishOrderEvents(outbox).Handle(placed, CancellationToken.None);

        outbox.Received(1).Add(Arg.Is<OrderPlacedIntegrationEvent>(integrationEvent =>
            integrationEvent.OrderId == order.Id && integrationEvent.Lines.Count == 2));
    }

    private OrderSaga Saga() => new(_orders, _unitOfWork);

    private Order Stored(Order order)
    {
        _orders.GetById(order.Id, Arg.Any<CancellationToken>()).Returns(order);
        return order;
    }
}
