using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using ShopEasy.BuildingBlocks;

namespace ShopEasy.Payment.Application;

public static class DependencyInjection
{
    public static readonly Assembly Assembly = typeof(DependencyInjection).Assembly;

    public static IServiceCollection AddPaymentApplication(this IServiceCollection services) =>
        services.AddApplicationCore(Assembly);
}
