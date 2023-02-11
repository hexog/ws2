using Microsoft.Extensions.DependencyInjection;

namespace Ws2.DependencyInjection;

[AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = true)]
public abstract class ServiceAttribute : Attribute
{
	protected ServiceAttribute(Type? service = null)
	{
		Service = service;
	}

	public Type? Service { get; set; }
	public abstract ServiceLifetime Lifetime { get; }
}

public class ScopedServiceAttribute : ServiceAttribute
{
	public ScopedServiceAttribute(Type? service = null) : base(service)
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
	public SingletonServiceAttribute(Type? service = null) : base(service)
	{
	}

	public override ServiceLifetime Lifetime => ServiceLifetime.Singleton;
}

public class SingletonServiceAttribute<TService> : SingletonServiceAttribute
{
	public SingletonServiceAttribute() : base(typeof(TService))
	{
	}
}

public class TransientServiceAttribute : ServiceAttribute
{
	public TransientServiceAttribute(Type? service = null) : base(service)
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

