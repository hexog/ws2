using System.Diagnostics;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Ws2.DependencyInjection;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceAttributeServiceCollectionExtensions
{
    public static IServiceCollection AddServicesByAttributesFromTypes(
        this IServiceCollection serviceCollection,
        IEnumerable<Type> types
    )
    {
        var context = ServiceAttributeBuildingContext.FromTypes(types);

        foreach (var type in context.Types)
        {
            var serviceAttributes = type.GetCustomAttributes<ServiceAttribute>().ToList();

            if (serviceAttributes.Count == 0)
            {
                continue;
            }

            var firstServiceAttribute = serviceAttributes[0];
            var lifetime = firstServiceAttribute.Lifetime;
            Debug.Assert(Enum.IsDefined(lifetime));

            if (serviceAttributes.Any(x => x.Lifetime != lifetime))
            {
                var lifetimes = string.Join(',', serviceAttributes.Select(x => x.Lifetime).Distinct());
                throw new ArgumentException($"Type {type.Name} is registered in different scopes: {lifetimes}");
            }

            if (lifetime == ServiceLifetime.Singleton)
            {
                AddSingletonService(serviceCollection, type, serviceAttributes, context);
            }
            else
            {
                foreach (var serviceAttribute in serviceAttributes)
                {
                    var service = context.FindServiceType(serviceAttribute) ?? type;
                    serviceCollection.TryAdd(new ServiceDescriptor(service, type, lifetime));
                }
            }
        }

        return serviceCollection;
    }

    public static IServiceCollection AddServicesByAttributes(
        this IServiceCollection serviceCollection,
        params Assembly[] assembliesToAdd
    )
    {
        var types = assembliesToAdd.SelectMany(x => x.DefinedTypes);
        return serviceCollection.AddServicesByAttributesFromTypes(types);
    }

    public static IServiceCollection AddServicesByAttributes(
        this IServiceCollection serviceCollection,
        Assembly assembly
    )
    {
        return serviceCollection.AddServicesByAttributesFromTypes(assembly.DefinedTypes);
    }

    private static void AddSingletonService(
        IServiceCollection serviceCollection,
        Type type,
        IEnumerable<ServiceAttribute> serviceAttributes,
        ServiceAttributeBuildingContext context
    )
    {
        serviceCollection.TryAdd(new ServiceDescriptor(type, type, ServiceLifetime.Singleton));
        foreach (var serviceAttribute in serviceAttributes)
        {
            var serviceType = context.FindServiceType(serviceAttribute);
            if (serviceType is null)
            {
                // already registered
                continue;
            }

            var singletonServiceAttribute = serviceAttribute as SingletonServiceAttribute;
            Debug.Assert(singletonServiceAttribute is not null);
            if (singletonServiceAttribute.InstanceSharing == SingletonServiceInstanceSharing.OwnInstance)
            {
                serviceCollection.TryAddEnumerable(ServiceDescriptor.Singleton(serviceType, type));
            }
            else
            {
                serviceCollection.TryAddEnumerable(
                    ServiceDescriptor.Singleton(serviceType, context.GetSingletonInstanceFactory(type))
                );
            }
        }
    }
}