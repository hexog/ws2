using Ws2.DependencyInjection.Abstractions;
using Ws2.DependencyInjection.BaseAttributes;

namespace Ws2.DependencyInjection.Registrars;

public class ScopedServiceAttributeRegistrar : IServiceAttributeRegistrar<ScopedServiceBaseAttribute>
{
    public void Register(
        ServiceAttributeRegistrarContext context,
        Type type,
        ScopedServiceBaseAttribute serviceAttribute
    )
    {
        context.Register(serviceAttribute.Lifetime, type, serviceAttribute);
    }
}