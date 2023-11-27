using Microsoft.Extensions.DependencyInjection;

namespace Ws2.DependencyInjection;

[AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = true)]
public abstract class ServiceAttribute : Attribute
{
    protected ServiceAttribute()
    {
    }

    protected ServiceAttribute(Type service)
    {
        Service = service;
    }

    protected ServiceAttribute(string serviceTypeName)
    {
        ServiceTypeName = serviceTypeName;
    }

    public Type? Service { get; set; }

    public string? ServiceTypeName { get; set; }

    public abstract ServiceLifetime Lifetime { get; }
}

public class ScopedServiceAttribute : ServiceAttribute
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

public class ScopedServiceAttribute<TService> : ScopedServiceAttribute
{
    public ScopedServiceAttribute() : base(typeof(TService))
    {
    }
}

public class SingletonServiceAttribute : ServiceAttribute
{
    public SingletonServiceInstanceSharing InstanceSharing { get; set; } =
        SingletonServiceInstanceSharing.SharedInstance;

    public SingletonServiceAttribute()
    {
    }

    public SingletonServiceAttribute(Type service) : base(service)
    {
    }

    public SingletonServiceAttribute(string serviceTypeName) : base(serviceTypeName)
    {
    }


    public override ServiceLifetime Lifetime => ServiceLifetime.Singleton;
}

public enum SingletonServiceInstanceSharing
{
    SharedInstance = 1,
    OwnInstance = 2
}

public class SingletonServiceAttribute<TService> : SingletonServiceAttribute
{
    public SingletonServiceAttribute() : base(typeof(TService))
    {
    }
}

public class TransientServiceAttribute : ServiceAttribute
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

    public override ServiceLifetime Lifetime => ServiceLifetime.Transient;
}

public class TransientServiceAttribute<TService> : TransientServiceAttribute
{
    public TransientServiceAttribute() : base(typeof(TService))
    {
    }
}