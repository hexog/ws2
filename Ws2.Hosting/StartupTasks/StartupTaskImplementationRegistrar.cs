using Microsoft.Extensions.DependencyInjection;
using Ws2.DependencyInjection.Abstractions;

namespace Ws2.Hosting.StartupTasks;

public class StartupTaskImplementationRegistrar : IServiceTypeImplementationRegistrar<IStartupTask>
{
    public void Register(IServiceAttributeRegistrarContext context, Type type)
    {
        context.ServiceCollection.AddStartupTask(type);
    }
}