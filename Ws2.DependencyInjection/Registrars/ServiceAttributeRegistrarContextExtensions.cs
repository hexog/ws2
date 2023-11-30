using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Ws2.DependencyInjection.Abstractions;

namespace Ws2.DependencyInjection.Registrars;

internal static class ServiceAttributeRegistrarContextExtensions
{
    public static void Register(
        this IServiceAttributeRegistrarContext context,
        ServiceLifetime serviceLifetime,
        Type implementation,
        ServiceAttribute serviceAttribute
    )
    {
        Debug.Assert(Enum.IsDefined(serviceLifetime));
        var service = serviceAttribute.Service
            ?? context.FIndType(serviceAttribute.ServiceTypeName)
            ?? implementation;
        context.ServiceCollection.TryAdd(new ServiceDescriptor(service, implementation, serviceLifetime));
    }
}