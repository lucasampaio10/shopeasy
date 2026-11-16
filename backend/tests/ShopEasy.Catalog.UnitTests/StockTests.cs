using ShopEasy.BuildingBlocks.Domain;
using ShopEasy.Catalog.Domain.Products;
using ShopEasy.Catalog.Domain.Reservations;

namespace ShopEasy.Catalog.UnitTests;

public class StockTests
{
    [Fact]
    public void Reserving_takes_units_off_the_shelf()
    {
        var product = ProductWithStock(5);

        product.Reserve(3);

        Assert.Equal(2, product.AvailableStock);
    }

    [Fact]
    public void Reserving_more_than_available_is_rejected()
    {
        var product = ProductWithStock(2);

        var exception = Assert.Throws<DomainException>(() => product.Reserve(3));

        Assert.Equal("product.insufficient_stock", exception.Code);
        Assert.Equal(2, product.AvailableStock);
    }

    [Fact]
    public void An_order_is_reserved_entirely_when_every_line_is_available()
    {
        var keyboard = ProductWithStock(5);
        var mouse = ProductWithStock(5);
        var orderId = Guid.NewGuid();

        var allocation = StockAllocator.Allocate(
            orderId,
            [new RequestedLine(keyboard.Id, 2), new RequestedLine(mouse.Id, 1)],
            Shelf(keyboard, mouse));

        Assert.False(allocation.IsRejected);
        Assert.Equal(orderId, allocation.Reservation!.OrderId);
        Assert.Equal(3, keyboard.AvailableStock);
        Assert.Equal(4, mouse.AvailableStock);
    }

    [Fact]
    public void Nothing_is_reserved_when_a_single_line_is_unavailable()
    {
        var keyboard = ProductWithStock(5);
        var mouse = ProductWithStock(0);

        var allocation = StockAllocator.Allocate(
            Guid.NewGuid(),
            [new RequestedLine(keyboard.Id, 2), new RequestedLine(mouse.Id, 1)],
            Shelf(keyboard, mouse));

        Assert.True(allocation.IsRejected);
        Assert.Contains("Estoque insuficiente", allocation.RejectionReason);
        Assert.Equal(5, keyboard.AvailableStock);
    }

    [Fact]
    public void Lines_of_the_same_product_are_checked_together()
    {
        var keyboard = ProductWithStock(3);

        var allocation = StockAllocator.Allocate(
            Guid.NewGuid(),
            [new RequestedLine(keyboard.Id, 2), new RequestedLine(keyboard.Id, 2)],
            Shelf(keyboard));

        Assert.True(allocation.IsRejected);
    }

    [Fact]
    public void An_unknown_product_rejects_the_order()
    {
        var allocation = StockAllocator.Allocate(Guid.NewGuid(), [new RequestedLine(Guid.NewGuid(), 1)], Shelf());

        Assert.True(allocation.IsRejected);
    }

    [Fact]
    public void A_reservation_is_released_only_once()
    {
        var keyboard = ProductWithStock(5);
        var reservation = StockAllocator
            .Allocate(Guid.NewGuid(), [new RequestedLine(keyboard.Id, 1)], Shelf(keyboard))
            .Reservation!;

        Assert.True(reservation.Release());
        Assert.False(reservation.Release());
    }

    internal static Product ProductWithStock(int stock) =>
        Product.Create("Teclado", $"SKU-{Guid.NewGuid():N}", "Teclado mecânico", new Money(400m), stock);

    internal static IReadOnlyDictionary<Guid, Product> Shelf(params Product[] products) =>
        products.ToDictionary(product => product.Id);
}
