using ShopEasy.BuildingBlocks.Domain;
using ShopEasy.Ordering.Domain.Orders;

namespace ShopEasy.Ordering.UnitTests;

public class OrderTests
{
    [Fact]
    public void A_placed_order_is_pending_and_announces_itself()
    {
        var order = OrderMother.Pending();

        Assert.Equal(OrderStatus.Pending, order.Status);
        var placed = Assert.IsType<OrderPlacedDomainEvent>(Assert.Single(order.DomainEvents));
        Assert.Equal(order.Id, placed.OrderId);
        Assert.Equal(2, placed.Items.Count);
    }

    [Fact]
    public void Items_of_the_same_product_become_a_single_line()
    {
        var product = Guid.NewGuid();

        var order = Order.Place(
            Guid.NewGuid(),
            new Address("Rua A, 1", "Fortaleza", "CE", "60000-000"),
            "tok_visa",
            [new RequestedItem(product, 1), new RequestedItem(product, 3)]);

        Assert.Equal(4, Assert.Single(order.Lines).Quantity);
    }

    [Fact]
    public void An_order_without_items_is_rejected()
    {
        var exception = Assert.Throws<DomainException>(() => Order.Place(
            Guid.NewGuid(),
            new Address("Rua A, 1", "Fortaleza", "CE", "60000-000"),
            "tok_visa",
            []));

        Assert.Equal("order.empty", exception.Code);
    }

    [Fact]
    public void Confirming_stock_prices_the_order_and_requests_payment()
    {
        var order = OrderMother.Pending();
        order.ClearDomainEvents();

        order.ConfirmStock(OrderMother.Prices());

        Assert.Equal(OrderStatus.AwaitingPayment, order.Status);
        Assert.Equal(new Money(700m), order.Total);
        var priced = Assert.IsType<OrderPricedDomainEvent>(Assert.Single(order.DomainEvents));
        Assert.Equal(order.Total, priced.Total);
    }

    [Fact]
    public void Confirming_stock_twice_has_no_additional_effect()
    {
        var order = OrderMother.AwaitingPayment();

        order.ConfirmStock(OrderMother.Prices());

        Assert.Empty(order.DomainEvents);
    }

    [Fact]
    public void Stock_confirmation_without_the_price_of_a_line_is_rejected()
    {
        var order = OrderMother.Pending();
        ItemPrice[] onlyKeyboard = [new ItemPrice(OrderMother.Keyboard, "Teclado", new Money(400m))];

        var exception = Assert.Throws<DomainException>(() => order.ConfirmStock(onlyKeyboard));

        Assert.Equal("order.missing_price", exception.Code);
    }

    [Fact]
    public void An_order_awaiting_payment_can_be_paid()
    {
        var order = OrderMother.AwaitingPayment();
        var paymentId = Guid.NewGuid();

        order.MarkAsPaid(paymentId);

        Assert.Equal(OrderStatus.Paid, order.Status);
        Assert.Equal(paymentId, order.PaymentId);
    }

    [Fact]
    public void An_order_cannot_be_paid_before_stock_is_confirmed()
    {
        var order = OrderMother.Pending();

        var exception = Assert.Throws<DomainException>(() => order.MarkAsPaid(Guid.NewGuid()));

        Assert.Equal("order.not_awaiting_payment", exception.Code);
    }

    [Fact]
    public void Cancelling_records_the_reason_and_announces_it_once()
    {
        var order = OrderMother.AwaitingPayment();

        order.Cancel("Card declined.");
        order.Cancel("Delivered again.");

        Assert.Equal(OrderStatus.Cancelled, order.Status);
        Assert.Equal("Card declined.", order.CancellationReason);
        Assert.IsType<OrderCancelledDomainEvent>(Assert.Single(order.DomainEvents));
    }

    [Fact]
    public void A_paid_order_cannot_be_cancelled()
    {
        var order = OrderMother.Paid();

        var exception = Assert.Throws<DomainException>(() => order.Cancel("Changed my mind."));

        Assert.Equal("order.already_paid", exception.Code);
    }
}
