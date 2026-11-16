using ShopEasy.BuildingBlocks.Messaging;
using ShopEasy.BuildingBlocks.Persistence;
using ShopEasy.Catalog.Domain.Products;
using ShopEasy.Catalog.Domain.Reservations;
using ShopEasy.Contracts;

namespace ShopEasy.Catalog.Application.Reservations;

/// <summary>Compensating action of the order saga: returns reserved units to the shelf.</summary>
internal sealed class ReleaseStockWhenOrderCancelled(
    IProductRepository products,
    IStockReservationRepository reservations,
    IUnitOfWork unitOfWork) : IIntegrationEventHandler<OrderCancelledIntegrationEvent>
{
    public async Task Handle(OrderCancelledIntegrationEvent orderCancelled, CancellationToken cancellationToken)
    {
        var reservation = await reservations.GetByOrderId(orderCancelled.OrderId, cancellationToken);
        if (reservation is null || !reservation.Release())
        {
            return;
        }

        var productIds = reservation.Lines.Select(line => line.ProductId).ToList();
        var reservedProducts = await products.GetByIds(productIds, cancellationToken);

        foreach (var line in reservation.Lines)
        {
            reservedProducts[line.ProductId].Release(line.Quantity);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
