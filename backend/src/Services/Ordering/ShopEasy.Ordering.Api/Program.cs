using ShopEasy.BuildingBlocks.Infrastructure.Persistence;
using ShopEasy.BuildingBlocks.Infrastructure.Web;
using ShopEasy.Ordering.Api;
using ShopEasy.Ordering.Application;
using ShopEasy.Ordering.Infrastructure;
using ShopEasy.Ordering.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddWebDefaults()
    .AddOrderingApplication()
    .AddOrderingInfrastructure(builder.Configuration);

var app = builder.Build();

app.UseWebDefaults();
app.MapOrderEndpoints();

await app.MigrateDatabaseAsync<OrderingDbContext>();
await app.RunAsync();
