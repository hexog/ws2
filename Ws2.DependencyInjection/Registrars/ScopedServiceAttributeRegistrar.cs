using Ws2.DependencyInjection.Abstractions;
using Ws2.DependencyInjection.LifetimeAttributes.Abstract;

namespace Ws2.DependencyInjection.Registrars;

public class ScopedServiceAttributeRegistrar : IServiceRegistrar
{
    public void TryRegister(IServiceRegistrarContext context, Type type)
    {
        var attributes = TypeAttributeHelper.GetTypeAttributes<ScopedServiceBaseAttribute>(type);
        if (attributes.Length <= 0)
        {
            return;
        }

        foreach (var attribute in attributes)
        {
            context.RegisterByServiceAttribute(type, attribute);
        }
    }
}