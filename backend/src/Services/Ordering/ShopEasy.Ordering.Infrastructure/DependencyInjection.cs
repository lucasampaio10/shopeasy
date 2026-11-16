using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ShopEasy.BuildingBlocks.Infrastructure;
using ShopEasy.Ordering.Application.Orders;
using ShopEasy.Ordering.Domain.Orders;
using ShopEasy.Ordering.Infrastructure.Persistence;

namespace ShopEasy.Ordering.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddOrderingInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<OrderingDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("Database")));

        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IOrderQueries, OrderQueries>();

        return services.AddInfrastructureCore<OrderingDbContext>(configuration, Application.DependencyInjection.Assembly);
    }
}
