using System.Diagnostics;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Ws2.DependencyInjection;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceAttributeServiceCollectionExtensions
{
    public static IServiceCollection AddServicesByAttributes(this IServiceCollection serviceCollection,
        params Assembly[] assembliesToAdd)
    {
        var context = new ServiceAttributeBuildingContext();

        var types = assembliesToAdd
            .SelectMany(x => x.DefinedTypes);

        foreach (var type in types)
        {
            var serviceAttributes =
                type.GetCustomAttributes(typeof(ServiceAttribute)).Cast<ServiceAttribute>().ToArray();

            if (serviceAttributes.Length == 0)
            {
                continue;
            }

            var firstServiceAttribute = serviceAttributes[0];
            var lifetime = firstServiceAttribute.Lifetime;
            Debug.Assert(Enum.IsDefined(lifetime));

            if (serviceAttributes.Any(x => x.Lifetime != lifetime))
            {
                continue;
            }

            if (lifetime == ServiceLifetime.Singleton)
            {
                AddSingletonService(serviceCollection, type, serviceAttributes, context);
            }
            else
            {
                foreach (var serviceAttribute in serviceAttributes)
                {
                    var service = serviceAttribute.Service ?? type;
                    serviceCollection.TryAdd(new ServiceDescriptor(service, type, lifetime));
                }
            }
        }

        return serviceCollection;
    }

    private static void AddSingletonService(IServiceCollection serviceCollection, TypeInfo type,
        ServiceAttribute[] serviceAttributes, ServiceAttributeBuildingContext context)
    {
        serviceCollection.TryAdd(new ServiceDescriptor(type, type, ServiceLifetime.Singleton));
        foreach (var serviceAttribute in serviceAttributes)
        {
            var factory = context.GetSingletonInstanceFactory(type);
            if (serviceAttribute.Service is { } service)
            {
                serviceCollection.TryAddEnumerable(
                    ServiceDescriptor.Singleton(serviceAttribute.Service, factory)
                );
            }
        }
    }
}