using Microsoft.Extensions.DependencyInjection;

namespace Ws2.DependencyInjection.LifetimeAttributes.Abstract;

public abstract class ScopedServiceBaseAttribute : ServiceAttribute
{
    public ScopedServiceBaseAttribute()
    {
    }

    public ScopedServiceBaseAttribute(Type service) : base(service)
    {
    }

    public ScopedServiceBaseAttribute(string serviceTypeName) : base(serviceTypeName)
    {
    }

    public override ServiceLifetime Lifetime => ServiceLifetime.Scoped;
}