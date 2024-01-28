using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Ws2.DependencyInjection.Abstractions;

namespace Ws2.Hosting.Registrars;

public class HostedServiceRegistrar : IServiceTypeImplementationRegistrar<IHostedService>
{
    public void Register(IServiceAttributeRegistrarContext context, Type type)
    {
        context.ServiceCollection.TryAddEnumerable(ServiceDescriptor.Singleton(typeof(IHostedService), type));
    }
}