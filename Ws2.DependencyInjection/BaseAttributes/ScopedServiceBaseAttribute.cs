using Microsoft.Extensions.DependencyInjection;
using Ws2.DependencyInjection.Abstractions;

namespace Ws2.DependencyInjection.BaseAttributes;

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