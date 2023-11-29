using Microsoft.Extensions.DependencyInjection;
using Ws2.DependencyInjection.Abstractions;

namespace Ws2.DependencyInjection.BaseAttributes;

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