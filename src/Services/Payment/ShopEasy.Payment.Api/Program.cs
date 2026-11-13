using ShopEasy.BuildingBlocks.Cqrs;
using ShopEasy.BuildingBlocks.Infrastructure.Persistence;
using ShopEasy.BuildingBlocks.Infrastructure.Web;
using ShopEasy.Payment.Application;
using ShopEasy.Payment.Application.Payments;
using ShopEasy.Payment.Infrastructure;
using ShopEasy.Payment.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddWebDefaults()
    .AddPaymentApplication()
    .AddPaymentInfrastructure(builder.Configuration);

var app = builder.Build();

app.UseWebDefaults();

app.MapGet("/api/payments/order/{orderId:guid}", async (Guid orderId, ISender sender, CancellationToken ct) =>
{
    var result = await sender.Send(new GetOrderPaymentQuery(orderId), ct);
    return result.ToHttpResult(Results.Ok);
}).WithTags("Payments");

await app.MigrateDatabaseAsync<PaymentDbContext>();
await app.RunAsync();
