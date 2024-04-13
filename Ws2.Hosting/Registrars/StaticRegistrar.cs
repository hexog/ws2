using Ws2.DependencyInjection.Abstractions;

namespace Ws2.Hosting.Registrars;

public class StaticRegistrar : IServiceRegistrar
{
    public void TryRegister(IServiceRegistrarContext context, Type type)
    {
        if (context.IsValidImplementationType(type) && type.IsAssignableTo(typeof(IStaticServiceRegistrar)))
        {
            var instance = Activator.CreateInstance(type);
            ((IStaticServiceRegistrar)instance!).Register(context.ServiceCollection);
        }
    }
}