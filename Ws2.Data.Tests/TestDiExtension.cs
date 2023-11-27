using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Ws2.Data.EntityHandlers;

namespace Ws2.Data.Tests;

public class TestDiExtension
{
	private IServiceProvider serviceProvider = null!;

	[SetUp]
	public void Setup()
	{
		var hostBuilder = Host.CreateDefaultBuilder();

		hostBuilder.ConfigureServices(x =>
		{
			x.AddDbContext<TestDbContext>()
				.AddEntityHandlers<TestDbContext>();
		});

		var app = hostBuilder.Build();
		serviceProvider = app.Services;
	}

	[Test]
	public void TestGetEntityHandler()
	{
		var scope = serviceProvider.CreateScope();
		var entityHandler = scope.ServiceProvider.GetRequiredService<EntityHandler<MyEntity>>();

		Assert.That(entityHandler, Is.Not.Null);
	}
}