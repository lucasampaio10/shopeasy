using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ShopEasy.BuildingBlocks.Infrastructure;
using ShopEasy.Catalog.Application.Products;
using ShopEasy.Catalog.Domain.Products;
using ShopEasy.Catalog.Domain.Reservations;
using ShopEasy.Catalog.Infrastructure.Persistence;

namespace ShopEasy.Catalog.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddCatalogInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<CatalogDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("Database")));

        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IStockReservationRepository, StockReservationRepository>();
        services.AddScoped<ICatalogQueries, CatalogQueries>();

        return services.AddInfrastructureCore<CatalogDbContext>(configuration, Application.DependencyInjection.Assembly);
    }
}
