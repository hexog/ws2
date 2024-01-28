using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Ws2.DependencyInjection.Abstractions;

namespace Ws2.Hosting.Registrars;

public class HostedServiceRegistrar : IServiceRegistrar
{
    public void TryRegister(IServiceRegistrarContext context, Type type)
    {
        if (type.IsAssignableTo(typeof(IHostedService)) && context.IsValidImplementationType(type))
        {
            context.ServiceCollection.TryAddEnumerable(ServiceDescriptor.Singleton(typeof(IHostedService), type));
        }
    }
}