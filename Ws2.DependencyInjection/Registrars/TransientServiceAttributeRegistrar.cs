using Ws2.DependencyInjection.Abstractions;
using Ws2.DependencyInjection.LifetimeAttributes.Abstract;

namespace Ws2.DependencyInjection.Registrars;

public class TransientServiceAttributeRegistrar : IServiceAttributeRegistrar<TransientServiceBaseAttribute>
{
    public void Register(
        IServiceAttributeRegistrarContext context,
        Type type,
        TransientServiceBaseAttribute serviceAttribute
    )
    {
        context.RegisterByServiceAttribute(serviceAttribute.Lifetime, type, serviceAttribute);
    }
}