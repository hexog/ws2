using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Ws2.DependencyInjection.Abstractions;
using Ws2.DependencyInjection.LifetimeAttributes.Abstract;

namespace Ws2.DependencyInjection.Registrars;

public class SingletonServiceAttributeRegistrar : IServiceRegistrar
{
    private readonly SingletonServiceAttributeBuildingContext buildingContext = new();

    public void Register(IServiceRegistrarContext context, Type type, SingletonServiceBaseAttribute[] serviceAttributes)
    {
        context.ServiceCollection.TryAdd(new ServiceDescriptor(type, type, ServiceLifetime.Singleton));

        foreach (var serviceAttribute in serviceAttributes)
        {
            var serviceType = serviceAttribute.Service ?? context.FindType(serviceAttribute.ServiceTypeName);
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

    public void TryRegister(IServiceRegistrarContext context, Type type)
    {
        var attributes = TypeAttributeHelper.GetTypeAttributes<SingletonServiceBaseAttribute>(type);
        if (attributes.Length != 0)
        {
            Register(
                context,
                type,
                attributes
            );
        }
    }
}