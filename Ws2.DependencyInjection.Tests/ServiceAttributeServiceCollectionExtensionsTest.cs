using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Ws2.DependencyInjection.LifetimeAttributes;

namespace Ws2.DependencyInjection.Tests;

public class ServiceAttributeServiceCollectionExtensionsTest
{
	private IServiceProvider serviceProvider = null!;

	[SetUp]
	public void Setup()
	{
		var hostBuilder = Host.CreateApplicationBuilder();
		hostBuilder.Services.AddServicesByAttributes(typeof(ServiceAttributeServiceCollectionExtensionsTest).Assembly);

		var app = hostBuilder.Build();
		serviceProvider = app.Services;
	}

	[Test]
	public void TestGetScopedService()
	{
		using var serviceScope = serviceProvider.CreateScope();
		var provider = serviceScope.ServiceProvider;
		var scopedServiceClass = provider.GetService<ScopedService>();
		Assert.That(scopedServiceClass, Is.Not.Null);
	}

	[Test]
	public void TestGetSingletonService()
	{
		var singletonServiceClass = serviceProvider.GetService<SingletonService>();
		Assert.That(singletonServiceClass, Is.Not.Null);
	}

	[Test]
	public void TestGetIgnoredService()
	{
		var ignoredServiceClass = serviceProvider.GetService<IgnoredService>();
		Assert.That(ignoredServiceClass, Is.Null);
	}

	[Test]
	public void TestGetAbstractedService()
	{
		using var serviceScope = serviceProvider.CreateScope();
		var abstractionService = serviceScope.ServiceProvider.GetService<IAbstractService>();
		Assert.That(abstractionService, Is.Not.Null);
	}

	[Test]
	public void TestGetMultiService()
	{
		using var serviceScope = serviceProvider.CreateScope();
		var serviceA = serviceScope.ServiceProvider.GetRequiredService<IVeryAbstractService>();
		var serviceB = serviceScope.ServiceProvider.GetRequiredService<IAnotherAbstractService>();

		Assert.That(serviceB.GetType(), Is.EqualTo(serviceA.GetType()));
	}
}

[ScopedService]
public class ScopedService
{
}

[SingletonService]
public class SingletonService
{
}

public class IgnoredService
{
}

public interface IAbstractService
{
}

[ScopedService<IAbstractService>]
public class AbstractService : IAbstractService
{
}

public interface IAnotherAbstractService
{
}

public interface IVeryAbstractService
{
}

[ScopedService(typeof(IAnotherAbstractService))]
[ScopedService<IVeryAbstractService>]
public class AbstractMultiService : IAnotherAbstractService, IVeryAbstractService
{
}
