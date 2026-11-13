using FluentValidation;
using ShopEasy.BuildingBlocks.Cqrs;
using ShopEasy.BuildingBlocks.Results;

namespace ShopEasy.Catalog.Application.Products;

public sealed record ListProductsQuery(string? Search, int Page = 1, int PageSize = 20)
    : IQuery<PagedResult<ProductResponse>>;

internal sealed class ListProductsValidator : AbstractValidator<ListProductsQuery>
{
    private const int MaxPageSize = 100;

    public ListProductsValidator()
    {
        RuleFor(query => query.Page).GreaterThan(0);
        RuleFor(query => query.PageSize).InclusiveBetween(1, MaxPageSize);
    }
}

internal sealed class ListProductsHandler(ICatalogQueries queries)
    : IQueryHandler<ListProductsQuery, PagedResult<ProductResponse>>
{
    public async Task<Result<PagedResult<ProductResponse>>> Handle(
        ListProductsQuery query,
        CancellationToken cancellationToken) =>
        await queries.ListProducts(query.Search, query.Page, query.PageSize, cancellationToken);
}

public sealed record GetProductQuery(Guid ProductId) : IQuery<ProductResponse>;

internal sealed class GetProductHandler(ICatalogQueries queries) : IQueryHandler<GetProductQuery, ProductResponse>
{
    public async Task<Result<ProductResponse>> Handle(GetProductQuery query, CancellationToken cancellationToken)
    {
        var product = await queries.FindProduct(query.ProductId, cancellationToken);

        return product is null ? ProductErrors.NotFound(query.ProductId) : product;
    }
}
