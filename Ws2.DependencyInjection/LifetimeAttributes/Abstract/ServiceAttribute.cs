using Microsoft.Extensions.DependencyInjection;

namespace Ws2.DependencyInjection.LifetimeAttributes.Abstract;

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

    public string? ServiceKey { get; set; }

    public Type? Service { get; set; }

    public string? ServiceTypeName { get; set; }

    public abstract ServiceLifetime Lifetime { get; }
}