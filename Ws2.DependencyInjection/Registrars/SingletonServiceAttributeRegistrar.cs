using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Ws2.DependencyInjection.Abstractions;
using Ws2.DependencyInjection.LifetimeAttributes.Abstract;

namespace Ws2.DependencyInjection.Registrars;

public class SingletonServiceAttributeRegistrar :
    IServiceAttributeRegistrar<SingletonServiceBaseAttribute>
{
    private readonly SingletonServiceAttributeBuildingContext buildingContext = new();

    public void Register(IServiceAttributeRegistrarContext context, Type type, SingletonServiceBaseAttribute serviceAttribute)
    {
        context.ServiceCollection.TryAdd(new ServiceDescriptor(type, type, ServiceLifetime.Singleton));
        var serviceType = serviceAttribute.Service ?? context.FIndType(serviceAttribute.ServiceTypeName);
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