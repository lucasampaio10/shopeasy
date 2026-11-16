using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ShopEasy.BuildingBlocks.Infrastructure.Persistence;

public static class DatabaseMigrator
{
    private const int MaxAttempts = 10;
    private static readonly TimeSpan RetryDelay = TimeSpan.FromSeconds(3);

    /// <summary>Applies pending migrations, waiting for the database container to accept connections.</summary>
    public static async Task MigrateDatabaseAsync<TContext>(this IHost host)
        where TContext : DbContext
    {
        await using var scope = host.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<TContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<TContext>>();

        for (var attempt = 1; ; attempt++)
        {
            try
            {
                await context.Database.MigrateAsync();
                return;
            }
            catch (Exception exception) when (attempt < MaxAttempts)
            {
                logger.LogWarning("Database is not ready yet ({Reason}); attempt {Attempt}.", exception.Message, attempt);
                await Task.Delay(RetryDelay);
            }
        }
    }
}
