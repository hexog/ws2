using Ws2.DependencyInjection.LifetimeAttributes.Abstract;

namespace Ws2.DependencyInjection.LifetimeAttributes;

public sealed class TransientServiceAttribute<TService>() : TransientServiceBaseAttribute(typeof(TService));

public sealed class TransientServiceAttribute : TransientServiceBaseAttribute
{
    public TransientServiceAttribute()
    {
    }

    public TransientServiceAttribute(Type service) : base(service)
    {
    }

    public TransientServiceAttribute(string serviceTypeName) : base(serviceTypeName)
    {
    }
}