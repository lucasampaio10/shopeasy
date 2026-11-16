using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace ShopEasy.BuildingBlocks.Infrastructure.Web;

public static class WebApplicationExtensions
{
    /// <summary>Cross-cutting HTTP concerns shared by every service: errors, OpenAPI and health.</summary>
    public static IServiceCollection AddWebDefaults(this IServiceCollection services)
    {
        services.AddProblemDetails();
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
        services.AddHealthChecks();

        return services;
    }

    public static WebApplication UseWebDefaults(this WebApplication app)
    {
        app.UseExceptionHandler();
        app.UseSwagger();
        app.UseSwaggerUI();
        app.MapHealthChecks("/health");

        return app;
    }
}
