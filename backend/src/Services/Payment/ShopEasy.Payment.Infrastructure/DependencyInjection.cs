using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ShopEasy.BuildingBlocks.Infrastructure;
using ShopEasy.Payment.Application.Payments;
using ShopEasy.Payment.Domain.Payments;
using ShopEasy.Payment.Infrastructure.Gateways;
using ShopEasy.Payment.Infrastructure.Persistence;

namespace ShopEasy.Payment.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddPaymentInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<PaymentDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("Database")));

        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<IPaymentQueries, PaymentQueries>();
        services.AddSingleton<IPaymentGateway, SimulatedPaymentGateway>();

        return services.AddInfrastructureCore<PaymentDbContext>(configuration, Application.DependencyInjection.Assembly);
    }
}
