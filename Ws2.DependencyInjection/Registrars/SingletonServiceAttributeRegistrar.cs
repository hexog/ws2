using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Ws2.DependencyInjection.Abstractions;
using Ws2.DependencyInjection.BaseAttributes;

namespace Ws2.DependencyInjection.Registrars;

public class SingletonServiceAttributeRegistrar :
    IServiceAttributeRegistrar<SingletonServiceBaseAttribute>
{
    private readonly SingletonServiceAttributeBuildingContext buildingContext = new();

    public void Register(ServiceAttributeRegistrarContext context, Type type, SingletonServiceBaseAttribute serviceAttribute)
    {
        context.ServiceCollection.TryAdd(new ServiceDescriptor(type, type, ServiceLifetime.Singleton));
        var serviceType = context.FindServiceType(serviceAttribute);
        if (serviceType is null)
        {
            return;
        }

        if (serviceAttribute.InstanceSharing == SingletonServiceInstanceSharing.OwnInstance)
        {
            context.ServiceCollection.TryAddEnumerable(ServiceDescriptor.Singleton(serviceType, type));
        }
        else
        {
            context.ServiceCollection.TryAddEnumerable(
                ServiceDescriptor.Singleton(serviceType, buildingContext.GetSingletonInstanceFactory(type))
            );
        }

    }
}