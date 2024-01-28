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
        Type implementationType,
        ServiceAttribute serviceAttribute
    )
    {
        if (!context.IsValidImplementationType(implementationType))
        {
            return;
        }

        var lifetime = serviceAttribute.Lifetime;
        Debug.Assert(Enum.IsDefined(lifetime));
        var serviceKey = serviceAttribute.ServiceKey;
        var serviceType = serviceAttribute.Service
            ?? context.FindType(serviceAttribute.ServiceTypeName);
        if (serviceType is null)
        {
            context.ServiceCollection.TryAdd(new ServiceDescriptor(implementationType, serviceKey, implementationType, lifetime));
        }
        else
        {
            context.ServiceCollection.TryAddEnumerable(new ServiceDescriptor(serviceType, serviceKey, implementationType, lifetime));
        }
    }
}