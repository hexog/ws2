using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Ws2pg.Extensions.DependencyInjection;

public static class AttributeServicesServiceCollectionExtensions
{
	public static IServiceCollection AddServicesByAttributes(this IServiceCollection serviceCollection,
															 params Assembly[] assembliesToAdd)
	{
		var types = assembliesToAdd
		   .SelectMany(x => x.DefinedTypes);

		foreach (var type in types)
		{
			var lifetime = TryExtractLifetimeAttribute(type);
			if (lifetime == null)
			{
				continue;
			}

			var services = type.ImplementedInterfaces.ToList();
			if (services.Count == 0)
			{
				services.Add(type);
			}

			foreach (var service in services)
			{
				var descriptor = new ServiceDescriptor(service, type, lifetime.Value);
				serviceCollection.Add(descriptor);
			}
		}

		return serviceCollection;

		static ServiceLifetime? TryExtractLifetimeAttribute(TypeInfo typeInfo)
		{
			var attributes = typeInfo.CustomAttributes.ToArray();

			if (attributes.Length == 0 || attributes.Any(x => x.AttributeType == typeof(IgnoreServiceAttribute)))
			{
				return null;
			}

			if (attributes.Any(x => x.AttributeType == typeof(ScopedServiceAttribute)))
			{
				return ServiceLifetime.Scoped;
			}

			if (attributes.Any(x => x.AttributeType == typeof(SingletonServiceAttribute)))
			{
				return ServiceLifetime.Singleton;
			}

			return null;
		}
	}
}
