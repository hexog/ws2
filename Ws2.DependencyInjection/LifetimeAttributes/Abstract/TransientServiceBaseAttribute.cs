using Microsoft.Extensions.DependencyInjection;

namespace Ws2.DependencyInjection.LifetimeAttributes.Abstract;

public abstract class TransientServiceBaseAttribute : ServiceAttribute
{
    public TransientServiceBaseAttribute()
    {
    }

    public TransientServiceBaseAttribute(Type service) : base(service)
    {
    }

    public TransientServiceBaseAttribute(string serviceTypeName) : base(serviceTypeName)
    {
    }

    public override ServiceLifetime Lifetime => ServiceLifetime.Transient;
}