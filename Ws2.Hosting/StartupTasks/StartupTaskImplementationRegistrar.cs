using Microsoft.Extensions.DependencyInjection;
using Ws2.DependencyInjection.Abstractions;

namespace Ws2.Hosting.StartupTasks;

public class StartupTaskImplementationRegistrar : IServiceRegistrar
{
    public void TryRegister(IServiceRegistrarContext context, Type type)
    {
        if (type.IsAssignableTo(typeof(IStartupTask)) && context.IsValidImplementationType(type))
        {
            context.ServiceCollection.AddStartupTask(type);
        }
    }
}