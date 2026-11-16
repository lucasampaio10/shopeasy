using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using ShopEasy.BuildingBlocks;

namespace ShopEasy.Catalog.Application;

public static class DependencyInjection
{
    public static readonly Assembly Assembly = typeof(DependencyInjection).Assembly;

    public static IServiceCollection AddCatalogApplication(this IServiceCollection services) =>
        services.AddApplicationCore(Assembly);
}
