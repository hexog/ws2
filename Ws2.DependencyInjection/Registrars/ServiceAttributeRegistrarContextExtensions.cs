using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Ws2.DependencyInjection.Abstractions;
using Ws2.DependencyInjection.LifetimeAttributes.Abstract;

namespace Ws2.DependencyInjection.Registrars;

internal static class ServiceAttributeRegistrarContextExtensions
{
    public static void RegisterByServiceAttribute(
        this IServiceRegistrarContext context,
        Type implementation,
        ServiceAttribute serviceAttribute
    )
    {
        if (!context.IsValidImplementationType(implementation))
        {
            return;
        }

        var serviceLifetime = serviceAttribute.Lifetime;
        Debug.Assert(Enum.IsDefined(serviceLifetime));
        var serviceKey = serviceAttribute.ServiceKey;
        var service = serviceAttribute.Service
            ?? context.FindType(serviceAttribute.ServiceTypeName);
        if (service is null)
        {
            context.ServiceCollection.TryAdd(new ServiceDescriptor(implementation, serviceKey, implementation, serviceLifetime));
        }
        else
        {
            context.ServiceCollection.TryAddEnumerable(new ServiceDescriptor(service, serviceKey, implementation, serviceLifetime));
        }
    }
}