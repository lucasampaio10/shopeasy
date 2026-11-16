using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using ShopEasy.BuildingBlocks.Cqrs;
using ShopEasy.BuildingBlocks.Domain;

namespace ShopEasy.BuildingBlocks;

public static class DependencyInjection
{
    /// <summary>Registers the request pipeline plus every handler and validator found in the assembly.</summary>
    public static IServiceCollection AddApplicationCore(this IServiceCollection services, Assembly applicationAssembly)
    {
        services.AddScoped<ISender, Sender>();
        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
        services.AddValidatorsFromAssembly(applicationAssembly, includeInternalTypes: true);
        services.AddImplementationsOf(typeof(IRequestHandler<,>), applicationAssembly);
        services.AddImplementationsOf(typeof(IDomainEventHandler<>), applicationAssembly);

        return services;
    }

    private static void AddImplementationsOf(
        this IServiceCollection services,
        Type openGenericInterface,
        Assembly assembly)
    {
        var registrations =
            from implementation in assembly.GetTypes()
            where implementation is { IsClass: true, IsAbstract: false }
            from contract in implementation.GetInterfaces()
            where contract.IsGenericType && contract.GetGenericTypeDefinition() == openGenericInterface
            select (contract, implementation);

        foreach (var (contract, implementation) in registrations)
        {
            services.AddScoped(contract, implementation);
        }
    }
}
