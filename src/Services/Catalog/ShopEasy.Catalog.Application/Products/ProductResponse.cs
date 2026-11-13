using ShopEasy.BuildingBlocks.Results;

namespace ShopEasy.Catalog.Application.Products;

public sealed record ProductResponse(
    Guid Id,
    string Name,
    string Sku,
    string Description,
    decimal Price,
    string Currency,
    int AvailableStock);

/// <summary>
/// Read side of the catalog. Queries skip the domain model and project straight to response
/// shapes, so they stay cheap and never load aggregates just to display them.
/// </summary>
public interface ICatalogQueries
{
    Task<PagedResult<ProductResponse>> ListProducts(
        string? search,
        int page,
        int pageSize,
        CancellationToken cancellationToken);

    Task<ProductResponse?> FindProduct(Guid id, CancellationToken cancellationToken);
}

internal static class ProductErrors
{
    public static Error NotFound(Guid id) => Error.NotFound("product.not_found", $"Product {id} was not found.");

    public static Error DuplicateSku(string sku) =>
        Error.Conflict("product.duplicate_sku", $"A product with SKU '{sku}' already exists.");
}
