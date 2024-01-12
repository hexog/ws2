using System.Reflection;
using Ws2.DependencyInjection;
using Ws2.DependencyInjection.Abstractions;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceAttributeServiceCollectionExtensions
{
    private static IEnumerable<Type> DefaultTypes => typeof(ServiceAttributeServiceCollectionExtensions).Assembly.DefinedTypes;

    public static IServiceCollection AddServicesByAttributesFromTypes(
        this IServiceCollection serviceCollection,
        IEnumerable<Type> types
    )
    {
        var typeList = types
            .Concat(serviceCollection.Where(x => x.ImplementationType is not null).Select(x => x.ImplementationType!))
            .Distinct()
            .ToList();
        var context = new ServiceAttributeRegistrarContext(serviceCollection, typeList);

        foreach (var type in context.Types)
        {
            var interfaces = type.GetInterfaces();
            foreach (var serviceType in interfaces)
            {
                foreach (var registrar in context.FindServiceImplementationRegistrars(serviceType))
                {
                    registrar.Register(context, type);
                }
            }

            if (type.BaseType is { } baseType)
            {
                foreach (var registrar in context.FindServiceImplementationRegistrars(baseType))
                {
                    registrar.Register(context, type);
                }
            }

            var serviceAttributes = type.GetCustomAttributes<ServiceAttribute>().ToList();

            if (serviceAttributes.Count == 0)
            {
                continue;
            }

            foreach (var serviceAttribute in serviceAttributes)
            {
                var serviceAttributeRegistered = false;
                foreach (var registrar in context.FindServiceAttributeRegistrars(serviceAttribute.GetType()))
                {
                    registrar.Register(context, type, serviceAttribute);
                    serviceAttributeRegistered = true;
                }

                if (!serviceAttributeRegistered
                    && ServiceAttributeCollectionPopulationOptions.Global.EnsureServiceAttributeHasRegistrar)
                {
                    AttributeServiceCollectionPopulationException.ThrowAttributeServiceNotRegistered(
                        serviceAttribute.GetType()
                    );
                }
            }
        }

        return serviceCollection;
    }

    public static IServiceCollection AddServicesByAttributes(
        this IServiceCollection serviceCollection,
        params Assembly[] assembliesToAdd
    )
    {
        var types = assembliesToAdd.SelectMany(x => x.DefinedTypes).Concat(DefaultTypes);
        return serviceCollection.AddServicesByAttributesFromTypes(types);
    }

    public static IServiceCollection AddServicesByAttributes(
        this IServiceCollection serviceCollection,
        Assembly assembly
    )
    {
        return serviceCollection.AddServicesByAttributesFromTypes(assembly.DefinedTypes.Concat(DefaultTypes));
    }

    public static IServiceCollection AddServicesByAttributes(
        this IServiceCollection serviceCollection,
        IEnumerable<Assembly> assembliesToAdd,
        bool excludeDefaultRegistrars = false
    )
    {
        IEnumerable<Type> types = assembliesToAdd.SelectMany(x => x.DefinedTypes);
        if (!excludeDefaultRegistrars)
        {
            types = types.Concat(DefaultTypes);
        }

        return serviceCollection.AddServicesByAttributesFromTypes(types);
    }
}