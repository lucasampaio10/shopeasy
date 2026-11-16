using ShopEasy.BuildingBlocks.Domain;
using ShopEasy.Ordering.Domain.Orders;

namespace ShopEasy.Ordering.UnitTests;

/// <summary>Builds orders in the state a test needs, keeping the tests about behaviour.</summary>
internal static class OrderMother
{
    public static readonly Guid Keyboard = Guid.NewGuid();
    public static readonly Guid Mouse = Guid.NewGuid();

    public static Order Pending() => Order.Place(
        Guid.NewGuid(),
        new Address("Av. Beira Mar, 1000", "Fortaleza", "CE", "60165-121"),
        "tok_visa",
        [new RequestedItem(Keyboard, 1), new RequestedItem(Mouse, 2)]);

    public static Order AwaitingPayment()
    {
        var order = Pending();
        order.ConfirmStock(Prices());
        order.ClearDomainEvents();

        return order;
    }

    public static Order Paid()
    {
        var order = AwaitingPayment();
        order.MarkAsPaid(Guid.NewGuid());

        return order;
    }

    public static IReadOnlyCollection<ItemPrice> Prices() =>
    [
        new ItemPrice(Keyboard, "Teclado", new Money(400m)),
        new ItemPrice(Mouse, "Mouse", new Money(150m)),
    ];
}
