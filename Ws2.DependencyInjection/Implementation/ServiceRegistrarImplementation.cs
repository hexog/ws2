using Microsoft.Extensions.DependencyInjection;
using Ws2.DependencyInjection.Abstractions;

namespace Ws2.DependencyInjection.Implementation;

public static class ServiceRegistrarImplementation
{
    public static void RegisterServices(
        IServiceCollection serviceCollection,
        IEnumerable<Type> types,
        IEnumerable<IServiceRegistrar> serviceRegistrars
    )
    {
        var typeList = types.ToList();
        var context = new ServiceRegistrarContext(
            serviceCollection,
            typeList,
            serviceRegistrars.ToList()
        );

        foreach (var type in context.Types)
        {
            foreach (var serviceRegistrar in context.ServiceRegistrars)
            {
                serviceRegistrar.TryRegister(context, type);
            }
        }
    }
}