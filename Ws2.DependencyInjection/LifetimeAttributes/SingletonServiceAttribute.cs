using Ws2.DependencyInjection.BaseAttributes;

namespace Ws2.DependencyInjection.LifetimeAttributes;

public sealed class SingletonServiceAttribute : SingletonServiceBaseAttribute
{
    public SingletonServiceAttribute()
    {
    }

    public SingletonServiceAttribute(Type service) : base(service)
    {
    }

    public SingletonServiceAttribute(string serviceTypeName) : base(serviceTypeName)
    {
    }
}

public sealed class SingletonServiceAttribute<TService> : SingletonServiceBaseAttribute
{
    public SingletonServiceAttribute() : base(typeof(TService))
    {
    }
}