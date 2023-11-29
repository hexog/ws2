using Ws2.DependencyInjection.Abstractions;
using Ws2.DependencyInjection.BaseAttributes;

namespace Ws2.DependencyInjection.Registrars;

public class TransientServiceAttributeRegistrar : IServiceAttributeRegistrar<TransientServiceBaseAttribute>
{
    public void Register(
        ServiceAttributeRegistrarContext context,
        Type type,
        TransientServiceBaseAttribute serviceAttribute
    )
    {
        context.Register(serviceAttribute.Lifetime, type, serviceAttribute);
    }
}