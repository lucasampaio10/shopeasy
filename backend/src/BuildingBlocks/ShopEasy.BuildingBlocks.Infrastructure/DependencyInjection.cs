using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ShopEasy.BuildingBlocks.Infrastructure.Messaging;
using ShopEasy.BuildingBlocks.Infrastructure.Outbox;
using ShopEasy.BuildingBlocks.Infrastructure.Persistence;
using ShopEasy.BuildingBlocks.Messaging;
using ShopEasy.BuildingBlocks.Persistence;

namespace ShopEasy.BuildingBlocks.Infrastructure;

public static class DependencyInjection
{
    /// <summary>
    /// Wires the unit of work, the transactional outbox and the RabbitMQ consumer for a service
    /// whose state lives in <typeparamref name="TContext"/>. Integration event handlers are
    /// discovered in the application assembly and subscribed automatically.
    /// </summary>
    public static IServiceCollection AddInfrastructureCore<TContext>(
        this IServiceCollection services,
        IConfiguration configuration,
        Assembly applicationAssembly)
        where TContext : DbContext
    {
        services.Configure<RabbitMqOptions>(configuration.GetSection(RabbitMqOptions.SectionName));
        services.AddSingleton<RabbitMqConnection>();
        services.AddSingleton<IMessageBroker, RabbitMqMessageBroker>();

        services.AddScoped<IUnitOfWork, UnitOfWork<TContext>>();
        services.AddScoped<IIntegrationEventOutbox, EfIntegrationEventOutbox<TContext>>();
        services.AddHostedService<OutboxPublisher<TContext>>();

        services.AddSingleton(services.SubscribeIntegrationEventHandlers(applicationAssembly));
        services.AddHostedService<RabbitMqConsumer>();

        return services;
    }

    private static EventSubscriptions SubscribeIntegrationEventHandlers(
        this IServiceCollection services,
        Assembly applicationAssembly)
    {
        var subscriptions = new EventSubscriptions();
        var subscribe = typeof(EventSubscriptions).GetMethod(nameof(EventSubscriptions.Add))!;

        var handlers =
            from implementation in applicationAssembly.GetTypes()
            where implementation is { IsClass: true, IsAbstract: false }
            from contract in implementation.GetInterfaces()
            where contract.IsGenericType && contract.GetGenericTypeDefinition() == typeof(IIntegrationEventHandler<>)
            select (contract, implementation);

        foreach (var (contract, implementation) in handlers)
        {
            services.AddScoped(contract, implementation);
            subscribe.MakeGenericMethod(contract.GetGenericArguments()[0]).Invoke(subscriptions, null);
        }

        return subscriptions;
    }
}
