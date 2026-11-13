using ShopEasy.BuildingBlocks.Messaging;
using ShopEasy.BuildingBlocks.Persistence;
using ShopEasy.Catalog.Domain.Products;
using ShopEasy.Catalog.Domain.Reservations;
using ShopEasy.Contracts;

namespace ShopEasy.Catalog.Application.Reservations;

internal sealed class ReserveStockWhenOrderPlaced(
    IProductRepository products,
    IStockReservationRepository reservations,
    IIntegrationEventOutbox outbox,
    IUnitOfWork unitOfWork) : IIntegrationEventHandler<OrderPlacedIntegrationEvent>
{
    public async Task Handle(OrderPlacedIntegrationEvent orderPlaced, CancellationToken cancellationToken)
    {
        var alreadyReserved = await reservations.GetByOrderId(orderPlaced.OrderId, cancellationToken) is not null;
        if (alreadyReserved)
        {
            return;
        }

        var requestedLines = orderPlaced.Lines
            .Select(line => new RequestedLine(line.ProductId, line.Quantity))
            .ToList();
        var productIds = requestedLines.Select(line => line.ProductId).Distinct().ToList();
        var orderedProducts = await products.GetByIds(productIds, cancellationToken);

        var allocation = StockAllocator.Allocate(orderPlaced.OrderId, requestedLines, orderedProducts);
        if (allocation.IsRejected)
        {
            outbox.Add(new StockRejectedIntegrationEvent(orderPlaced.OrderId, allocation.RejectionReason!));
        }
        else
        {
            reservations.Add(allocation.Reservation!);
            outbox.Add(ToStockReserved(allocation.Reservation!, orderedProducts));
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static StockReservedIntegrationEvent ToStockReserved(
        StockReservation reservation,
        IReadOnlyDictionary<Guid, Product> products)
    {
        var pricedLines = reservation.Lines
            .Select(line =>
            {
                var product = products[line.ProductId];
                return new PricedOrderLineContract(product.Id, product.Name, line.Quantity, product.Price.Amount);
            })
            .ToList();
        var currency = products[reservation.Lines.First().ProductId].Price.Currency;

        return new StockReservedIntegrationEvent(reservation.OrderId, pricedLines, currency);
    }
}
