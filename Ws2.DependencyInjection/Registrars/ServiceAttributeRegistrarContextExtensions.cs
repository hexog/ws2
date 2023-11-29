using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Ws2.DependencyInjection.Abstractions;

namespace Ws2.DependencyInjection.Registrars;

internal static class ServiceAttributeRegistrarContextExtensions
{
    public static void Register(
        this ServiceAttributeRegistrarContext context,
        ServiceLifetime serviceLifetime,
        Type implementation,
        ServiceAttribute serviceAttribute
    )
    {
        Debug.Assert(Enum.IsDefined(serviceLifetime));
        var service = context.FindServiceType(serviceAttribute) ?? implementation;
        context.ServiceCollection.TryAdd(new ServiceDescriptor(service, implementation, serviceLifetime));
    }

    public static Type? FindServiceType(this ServiceAttributeRegistrarContext context, ServiceAttribute serviceAttribute)
    {
        var service = serviceAttribute.Service;
        if (serviceAttribute.Service is null && serviceAttribute.ServiceTypeName is not null)
        {
            if (context.FullNameToType.TryGetValue(serviceAttribute.ServiceTypeName, out service))
            {
                return service;
            }

            var types = context.NameToType[serviceAttribute.ServiceTypeName];
            return types.SingleOrDefault();
        }

        return service;
    }
}