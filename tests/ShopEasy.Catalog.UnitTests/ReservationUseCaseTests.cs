using NSubstitute;
using ShopEasy.BuildingBlocks.Messaging;
using ShopEasy.BuildingBlocks.Persistence;
using ShopEasy.Catalog.Application.Reservations;
using ShopEasy.Catalog.Domain.Products;
using ShopEasy.Catalog.Domain.Reservations;
using ShopEasy.Contracts;

namespace ShopEasy.Catalog.UnitTests;

public class ReservationUseCaseTests
{
    private readonly IProductRepository _products = Substitute.For<IProductRepository>();
    private readonly IStockReservationRepository _reservations = Substitute.For<IStockReservationRepository>();
    private readonly IIntegrationEventOutbox _outbox = Substitute.For<IIntegrationEventOutbox>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    [Fact]
    public async Task A_placed_order_with_available_stock_is_reserved_and_priced()
    {
        var keyboard = OnTheShelf(StockTests.ProductWithStock(5));
        var orderPlaced = new OrderPlacedIntegrationEvent(Guid.NewGuid(), [new OrderLineContract(keyboard.Id, 2)]);

        await ReserveHandler().Handle(orderPlaced, CancellationToken.None);

        Assert.Equal(3, keyboard.AvailableStock);
        _reservations.Received(1).Add(Arg.Is<StockReservation>(reservation => reservation.OrderId == orderPlaced.OrderId));
        _outbox.Received(1).Add(Arg.Is<StockReservedIntegrationEvent>(reserved =>
            reserved.OrderId == orderPlaced.OrderId
            && reserved.Lines.Single().UnitPrice == 400m
            && reserved.Currency == "BRL"));
    }

    [Fact]
    public async Task A_placed_order_without_stock_is_rejected_and_nothing_is_held()
    {
        var keyboard = OnTheShelf(StockTests.ProductWithStock(1));
        var orderPlaced = new OrderPlacedIntegrationEvent(Guid.NewGuid(), [new OrderLineContract(keyboard.Id, 2)]);

        await ReserveHandler().Handle(orderPlaced, CancellationToken.None);

        Assert.Equal(1, keyboard.AvailableStock);
        _reservations.DidNotReceive().Add(Arg.Any<StockReservation>());
        _outbox.Received(1).Add(Arg.Is<StockRejectedIntegrationEvent>(rejected => rejected.OrderId == orderPlaced.OrderId));
    }

    [Fact]
    public async Task The_same_order_delivered_twice_is_reserved_once()
    {
        var keyboard = OnTheShelf(StockTests.ProductWithStock(5));
        var orderPlaced = new OrderPlacedIntegrationEvent(Guid.NewGuid(), [new OrderLineContract(keyboard.Id, 2)]);
        _reservations.Add(Arg.Do<StockReservation>(reservation =>
            _reservations.GetByOrderId(reservation.OrderId, Arg.Any<CancellationToken>()).Returns(reservation)));

        await ReserveHandler().Handle(orderPlaced, CancellationToken.None);
        await ReserveHandler().Handle(orderPlaced, CancellationToken.None);

        Assert.Equal(3, keyboard.AvailableStock);
        _outbox.Received(1).Add(Arg.Any<IntegrationEvent>());
    }

    [Fact]
    public async Task A_cancelled_order_returns_its_units_to_the_shelf()
    {
        var keyboard = OnTheShelf(StockTests.ProductWithStock(5));
        var orderId = Guid.NewGuid();
        var reservation = StockAllocator
            .Allocate(orderId, [new RequestedLine(keyboard.Id, 2)], StockTests.Shelf(keyboard))
            .Reservation!;
        _reservations.GetByOrderId(orderId, Arg.Any<CancellationToken>()).Returns(reservation);
        var handler = new ReleaseStockWhenOrderCancelled(_products, _reservations, _unitOfWork);

        await handler.Handle(new OrderCancelledIntegrationEvent(orderId, "Card declined."), CancellationToken.None);
        await handler.Handle(new OrderCancelledIntegrationEvent(orderId, "Card declined."), CancellationToken.None);

        Assert.Equal(5, keyboard.AvailableStock);
        Assert.Equal(ReservationStatus.Released, reservation.Status);
    }

    private ReserveStockWhenOrderPlaced ReserveHandler() => new(_products, _reservations, _outbox, _unitOfWork);

    private Product OnTheShelf(Product product)
    {
        _products.GetByIds(Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(StockTests.Shelf(product));
        return product;
    }
}
