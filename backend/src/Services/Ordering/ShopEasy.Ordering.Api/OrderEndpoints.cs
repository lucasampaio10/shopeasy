using ShopEasy.BuildingBlocks.Cqrs;
using ShopEasy.BuildingBlocks.Infrastructure.Web;
using ShopEasy.Ordering.Application.Orders;

namespace ShopEasy.Ordering.Api;

internal static class OrderEndpoints
{
    public static void MapOrderEndpoints(this IEndpointRouteBuilder routes)
    {
        var orders = routes.MapGroup("/api/orders").WithTags("Orders");

        orders.MapPost("/", async (PlaceOrderCommand command, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(command, ct);

            // 202: the order was accepted, but stock and payment are settled asynchronously.
            return result.ToHttpResult(id => Results.AcceptedAtRoute("GetOrder", new { id }, new { id }));
        });

        orders.MapGet("/{id:guid}", async (Guid id, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new GetOrderQuery(id), ct);
            return result.ToHttpResult(Results.Ok);
        }).WithName("GetOrder");

        orders.MapGet("/", async (Guid customerId, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new ListCustomerOrdersQuery(customerId), ct);
            return result.ToHttpResult(Results.Ok);
        });

        orders.MapPost("/{id:guid}/cancel", async (Guid id, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new CancelOrderCommand(id), ct);
            return result.ToHttpResult(Results.NoContent);
        });
    }
}
