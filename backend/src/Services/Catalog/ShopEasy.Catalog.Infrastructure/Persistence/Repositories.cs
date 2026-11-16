using Microsoft.EntityFrameworkCore;
using ShopEasy.Catalog.Domain.Products;
using ShopEasy.Catalog.Domain.Reservations;

namespace ShopEasy.Catalog.Infrastructure.Persistence;

internal sealed class ProductRepository(CatalogDbContext context) : IProductRepository
{
    public Task<Product?> GetById(Guid id, CancellationToken cancellationToken) =>
        context.Products.FirstOrDefaultAsync(product => product.Id == id, cancellationToken);

    public async Task<IReadOnlyDictionary<Guid, Product>> GetByIds(
        IReadOnlyCollection<Guid> ids,
        CancellationToken cancellationToken) =>
        await context.Products
            .Where(product => ids.Contains(product.Id))
            .ToDictionaryAsync(product => product.Id, cancellationToken);

    public Task<bool> ExistsWithSku(string sku, CancellationToken cancellationToken) =>
        context.Products.AnyAsync(product => product.Sku == sku, cancellationToken);

    public void Add(Product product) => context.Products.Add(product);
}

internal sealed class StockReservationRepository(CatalogDbContext context) : IStockReservationRepository
{
    public Task<StockReservation?> GetByOrderId(Guid orderId, CancellationToken cancellationToken) =>
        context.StockReservations.FirstOrDefaultAsync(reservation => reservation.Id == orderId, cancellationToken);

    public void Add(StockReservation reservation) => context.StockReservations.Add(reservation);
}
