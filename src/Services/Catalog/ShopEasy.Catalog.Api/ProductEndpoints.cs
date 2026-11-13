using ShopEasy.BuildingBlocks.Cqrs;
using ShopEasy.BuildingBlocks.Infrastructure.Web;
using ShopEasy.Catalog.Application.Products;

namespace ShopEasy.Catalog.Api;

internal static class ProductEndpoints
{
    public static void MapProductEndpoints(this IEndpointRouteBuilder routes)
    {
        var products = routes.MapGroup("/api/products").WithTags("Products");

        products.MapGet("/", async (string? search, int? page, int? pageSize, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new ListProductsQuery(search, page ?? 1, pageSize ?? 20), ct);
            return result.ToHttpResult(Results.Ok);
        });

        products.MapGet("/{id:guid}", async (Guid id, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new GetProductQuery(id), ct);
            return result.ToHttpResult(Results.Ok);
        }).WithName("GetProduct");

        products.MapPost("/", async (CreateProductCommand command, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(command, ct);
            return result.ToHttpResult(id => Results.CreatedAtRoute("GetProduct", new { id }, new { id }));
        });

        products.MapPost("/{id:guid}/restock", async (Guid id, RestockRequest request, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new RestockProductCommand(id, request.Quantity), ct);
            return result.ToHttpResult(Results.NoContent);
        });
    }

    private sealed record RestockRequest(int Quantity);
}
