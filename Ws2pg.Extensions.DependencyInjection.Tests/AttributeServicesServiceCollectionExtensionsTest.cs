using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Ws2pg.Extensions.DependencyInjection.Tests;

public class AttributeServicesServiceCollectionExtensionsTest
{
	private IServiceProvider serviceProvider = null!;

	[SetUp]
	public void Setup()
	{
		var hostBuilder = Host.CreateDefaultBuilder();

		hostBuilder.ConfigureServices(
			x => x.AddServicesByAttributes(typeof(AttributeServicesServiceCollectionExtensionsTest).Assembly)
		);

		var app = hostBuilder.Build();
		serviceProvider = app.Services;
	}

	[Test]
	public void TestGetScopedService()
	{
		var serviceScope = serviceProvider.CreateScope();
		var provider = serviceScope.ServiceProvider;
		var scopedServiceClass = provider.GetService<ScopedServiceClass>();
		Assert.That(scopedServiceClass, Is.Not.Null);
	}

	[Test]
	public void TestGetSingletonService()
	{
		var singletonServiceClass = serviceProvider.GetService<SingletonServiceClass>();
		Assert.That(singletonServiceClass, Is.Not.Null);
	}

	[Test]
	public void TestGetIgnoredService()
	{
		var ignoredServiceClass = serviceProvider.GetService<IgnoredServiceClass>();
		Assert.That(ignoredServiceClass, Is.Null);
	}

	[Test]
	public void TestGetExplicitlyIgnoredService()
	{
		var explicitlyIgnoredServiceClass = serviceProvider.GetService<ExplicitlyIgnoredServiceClass>();
		Assert.That(explicitlyIgnoredServiceClass, Is.Null);
	}

	[Test]
	public void GetAbstractedService()
	{
		var serviceScope = serviceProvider.CreateScope();
		var abstractionService = serviceScope.ServiceProvider.GetService<IAbstractionService>();
		Assert.That(abstractionService, Is.Not.Null);
	}
}

[ScopedService]
public class ScopedServiceClass
{
}

[SingletonService]
public class SingletonServiceClass
{
}

[ScopedService]
[IgnoreService]
public class ExplicitlyIgnoredServiceClass
{
}

public class IgnoredServiceClass
{
}

public interface IAbstractionService
{
}

[ScopedService]
public class AbstractionService : IAbstractionService
{
}
