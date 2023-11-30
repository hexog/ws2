using System.Reflection;
using Ws2.DependencyInjection;
using Ws2.DependencyInjection.Abstractions;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceAttributeServiceCollectionExtensions
{
    private static IEnumerable<Type> DefaultTypes =>
        typeof(ServiceAttributeServiceCollectionExtensions).Assembly.DefinedTypes;

    public static IServiceCollection AddServicesByAttributesFromTypes(
        this IServiceCollection serviceCollection,
        IEnumerable<Type> types
    )
    {
        var typeList = types
            .Concat(serviceCollection.Where(x => x.ImplementationType is not null).Select(x => x.ImplementationType!))
            .Distinct()
            .ToList();
        var context = new ServiceAttributeRegistrarContext(
            serviceCollection,
            typeList,
            typeList.ToLookup(x => x.Name),
            typeList
                .Select(x => (x.FullName, Type: x))
                .Where(x => x.FullName is not null)
                .ToDictionary(x => x.FullName!, x => x.Type),
            typeList
                .Where(x => x.IsAssignableTo(typeof(IServiceAttributeRegistrar)))
                .Select(x => (IServiceAttributeRegistrar)Activator.CreateInstance(x)!)
                .ToList(),
            typeList
                .Where(x => x.IsAssignableTo(typeof(IServiceTypeImplementationRegistrar)))
                .Select(x => (IServiceTypeImplementationRegistrar)Activator.CreateInstance(x)!)
                .ToList()
        );

        foreach (var type in context.Types)
        {
            var interfaces = type.GetInterfaces();
            foreach (var baseType in interfaces)
            {
                var registrar = context.FindServiceImplementationRegistrar(baseType);
                if (registrar is not null)
                {
                    registrar.Register(context, type);
                }
            }

            if (type.BaseType is not null)
            {
                var registrar = context.FindServiceImplementationRegistrar(type.BaseType);
                if (registrar is not null)
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
                var registrar = context.FindServiceAttributeRegistrar(serviceAttribute.GetType());
                if (registrar is null)
                {
                    throw new ArgumentException(
                        $"Registrar not found for service attribute: {serviceAttribute.GetType().Name}");
                }

                registrar.Register(context, type, serviceAttribute);
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