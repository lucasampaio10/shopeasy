using Microsoft.EntityFrameworkCore;
using ShopEasy.BuildingBlocks.Results;
using ShopEasy.Catalog.Application.Products;
using ShopEasy.Catalog.Domain.Products;

namespace ShopEasy.Catalog.Infrastructure.Persistence;

internal sealed class CatalogQueries(CatalogDbContext context) : ICatalogQueries
{
    public async Task<PagedResult<ProductResponse>> ListProducts(
        string? search,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var products = context.Products.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var pattern = $"%{search.Trim()}%";
            products = products.Where(product =>
                EF.Functions.ILike(product.Name, pattern) || EF.Functions.ILike(product.Sku, pattern));
        }

        var totalItems = await products.CountAsync(cancellationToken);
        var items = await ToResponse(products.OrderBy(product => product.Name))
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<ProductResponse>(items, page, pageSize, totalItems);
    }

    public Task<ProductResponse?> FindProduct(Guid id, CancellationToken cancellationToken) =>
        ToResponse(context.Products.AsNoTracking().Where(product => product.Id == id))
            .FirstOrDefaultAsync(cancellationToken);

    private static IQueryable<ProductResponse> ToResponse(IQueryable<Product> products) =>
        products.Select(product => new ProductResponse(
            product.Id,
            product.Name,
            product.Sku,
            product.Description,
            product.Price.Amount,
            product.Price.Currency,
            product.AvailableStock));
}
