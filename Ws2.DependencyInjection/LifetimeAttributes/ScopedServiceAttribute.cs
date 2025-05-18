using Microsoft.Extensions.DependencyInjection;
using Ws2.DependencyInjection.LifetimeAttributes.Abstract;

namespace Ws2.DependencyInjection.LifetimeAttributes;

public sealed class ScopedServiceAttribute : ScopedServiceBaseAttribute
{
    public ScopedServiceAttribute()
    {
    }

    public ScopedServiceAttribute(Type service) : base(service)
    {
    }

    public ScopedServiceAttribute(string serviceTypeName) : base(serviceTypeName)
    {
    }

    public override ServiceLifetime Lifetime => ServiceLifetime.Scoped;
}

public sealed class ScopedServiceAttribute<TService>() : ScopedServiceBaseAttribute(typeof(TService))
{
    public override ServiceLifetime Lifetime => ServiceLifetime.Scoped;
}