using Microsoft.Extensions.DependencyInjection;
using Ws2.DependencyInjection.Abstractions;

namespace Ws2.DependencyInjection.BaseAttributes;

public abstract class SingletonServiceBaseAttribute : ServiceAttribute
{
    public SingletonServiceInstanceSharing InstanceSharing { get; set; } =
        SingletonServiceInstanceSharing.SharedInstance;

    public SingletonServiceBaseAttribute()
    {
    }

    public SingletonServiceBaseAttribute(Type service) : base(service)
    {
    }

    public SingletonServiceBaseAttribute(string serviceTypeName) : base(serviceTypeName)
    {
    }


    public override ServiceLifetime Lifetime => ServiceLifetime.Singleton;
}