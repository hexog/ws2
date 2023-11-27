using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Ws2.Data;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class DataServiceCollectionExtensions
{
	public static IServiceCollection AddEntityHandlers<TDbContext>(this IServiceCollection serviceCollection)
		where TDbContext : DbContext
	{
		serviceCollection.TryAdd(new ServiceDescriptor(
			typeof(DbContextAccessor),
			p => new DbContextAccessor(p.GetRequiredService<TDbContext>()),
			ServiceLifetime.Scoped
		));

		serviceCollection.AddScoped(typeof(EntityHandler<>), typeof(EntityHandler<>));
		return serviceCollection;
	}
}