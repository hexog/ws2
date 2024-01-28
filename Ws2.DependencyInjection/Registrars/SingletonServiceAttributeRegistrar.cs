using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Ws2.DependencyInjection.Abstractions;
using Ws2.DependencyInjection.LifetimeAttributes.Abstract;

namespace Ws2.DependencyInjection.Registrars;

public class SingletonServiceAttributeRegistrar : IServiceRegistrar
{
    private readonly SingletonServiceAttributeBuildingContext buildingContext = new();

    public void Register(IServiceRegistrarContext context, Type implementationType, SingletonServiceBaseAttribute[] serviceAttributes)
    {
        context.ServiceCollection.TryAdd(new ServiceDescriptor(implementationType, implementationType, ServiceLifetime.Singleton));

        foreach (var serviceAttribute in serviceAttributes)
        {
            var serviceKey = serviceAttribute.ServiceKey;
            if (serviceKey is not null)
            {
                context.ServiceCollection.TryAdd(
                    new ServiceDescriptor(implementationType, serviceKey, implementationType, ServiceLifetime.Singleton)
                );
            }

            var serviceType = serviceAttribute.Service ?? context.FindType(serviceAttribute.ServiceTypeName);
            if (serviceType is null)
            {
                continue;
            }

            if (serviceAttribute.InstanceSharing == SingletonServiceInstanceSharing.OwnInstance)
            {
                context.ServiceCollection.TryAddEnumerable(
                    new ServiceDescriptor(serviceType, serviceKey, implementationType, ServiceLifetime.Singleton)
                );
            }
            else
            {
                if (serviceAttribute.InstanceSharing == SingletonServiceInstanceSharing.KeyedInstance)
                {
                    context.ServiceCollection.TryAddEnumerable(
                        new ServiceDescriptor(
                            serviceType,
                            serviceKey,
                            factory: buildingContext.GetKeyedSingletonKeyedInstanceFactory(implementationType),
                            lifetime: ServiceLifetime.Singleton
                        )
                    );
                }
                else
                {
                    if (serviceKey is null)
                    {
                        context.ServiceCollection.TryAddEnumerable(
                            new ServiceDescriptor(
                                serviceType,
                                factory: buildingContext.GetSingletonInstanceFactory(implementationType),
                                lifetime: ServiceLifetime.Singleton
                            )
                        );
                    }
                    else
                    {
                        context.ServiceCollection.TryAddEnumerable(
                            new ServiceDescriptor(
                                serviceType,
                                serviceKey,
                                factory: buildingContext.GetKeyedSingletonInstanceFactory(implementationType),
                                lifetime: ServiceLifetime.Singleton
                            )
                        );
                    }
                }
            }
        }
    }

    public void TryRegister(IServiceRegistrarContext context, Type type)
    {
        if (!context.IsValidImplementationType(type))
        {
            return;
        }

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