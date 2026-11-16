using ShopEasy.BuildingBlocks.Infrastructure.Persistence;
using ShopEasy.BuildingBlocks.Infrastructure.Web;
using ShopEasy.Catalog.Api;
using ShopEasy.Catalog.Application;
using ShopEasy.Catalog.Infrastructure;
using ShopEasy.Catalog.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddWebDefaults()
    .AddCatalogApplication()
    .AddCatalogInfrastructure(builder.Configuration);

var app = builder.Build();

app.UseWebDefaults();
app.MapProductEndpoints();

await app.MigrateDatabaseAsync<CatalogDbContext>();
await app.SeedCatalogAsync();
await app.RunAsync();
