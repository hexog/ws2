using System.Reflection;
using Ws2.DependencyInjection;
using Ws2.DependencyInjection.Abstractions;
using Ws2.DependencyInjection.Registrars;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceAttributeServiceCollectionExtensions
{
    public static readonly IServiceAttributeRegistrar[] DefaultAttributeRegistrars =
    {
        new SingletonServiceAttributeRegistrar(),
        new ScopedServiceAttributeRegistrar(),
        new TransientServiceAttributeRegistrar()
    };

    public static IServiceCollection AddServicesByAttributesFromTypes(
        this IServiceCollection serviceCollection,
        IEnumerable<Type> types,
        IReadOnlyCollection<IServiceAttributeRegistrar> attributeRegistrars,
        IReadOnlyCollection<IServiceTypeImplementationRegistrar> implementationRegistrars
    )
    {
        var typeList = types
            .Concat(serviceCollection.Where(x => x.ImplementationType is not null).Select(x => x.ImplementationType!))
            .Distinct()
            .ToList();
        var context = new ServiceAttributeRegistrarContext(
            serviceCollection,
            typeList,
            attributeRegistrars,
            implementationRegistrars
        );

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

            foreach (var serviceAttribute in type.GetCustomAttributes())
            {
                foreach (var registrar in context.FindServiceAttributeRegistrars(serviceAttribute.GetType()))
                {
                    registrar.Register(context, type, serviceAttribute);
                }
            }
        }

        return serviceCollection;
    }

    public static IServiceCollection AddServicesFromAssembly(
        this IServiceCollection serviceCollection,
        params Assembly[] assembliesToAdd
    )
    {
        var types = assembliesToAdd.SelectMany(x => x.DefinedTypes);
        return serviceCollection.AddServicesByAttributesFromTypes(
            types,
            DefaultAttributeRegistrars,
            Array.Empty<IServiceTypeImplementationRegistrar>()
        );
    }

    public static IServiceCollection AddServicesFromAssembly(
        this IServiceCollection serviceCollection,
        IEnumerable<Assembly> assembliesToAdd,
        IReadOnlyCollection<IServiceAttributeRegistrar> attributeRegistrars,
        IReadOnlyCollection<IServiceTypeImplementationRegistrar> implementationRegistrars
    )
    {
        var types = assembliesToAdd.SelectMany(x => x.DefinedTypes);
        return serviceCollection.AddServicesByAttributesFromTypes(types, attributeRegistrars, implementationRegistrars);
    }

    public static IServiceCollection AddServicesFromAssembly(
        this IServiceCollection serviceCollection,
        Assembly assembly
    )
    {
        return serviceCollection.AddServicesByAttributesFromTypes(
            assembly.DefinedTypes,
            DefaultAttributeRegistrars,
            Array.Empty<IServiceTypeImplementationRegistrar>()
        );
    }

    public static IServiceCollection AddServicesFromAssembly(
        this IServiceCollection serviceCollection,
        Assembly assembly,
        IEnumerable<IServiceAttributeRegistrar> attributeRegistrars,
        IReadOnlyCollection<IServiceTypeImplementationRegistrar> implementationRegistrars
    )
    {
        return serviceCollection.AddServicesByAttributesFromTypes(
            assembly.DefinedTypes,
            attributeRegistrars.Concat(DefaultAttributeRegistrars).ToList(),
            implementationRegistrars
        );
    }

    public static IServiceCollection AddServicesFromAssemblyWithoutDefaultRegistrars(
        this IServiceCollection serviceCollection,
        IEnumerable<Assembly> assembliesToAdd,
        IEnumerable<IServiceAttributeRegistrar> attributeRegistrars,
        IReadOnlyCollection<IServiceTypeImplementationRegistrar> implementationRegistrars
    )
    {
        var types = assembliesToAdd.SelectMany(x => x.DefinedTypes);
        return serviceCollection.AddServicesByAttributesFromTypes(
            types,
            attributeRegistrars.Concat(DefaultAttributeRegistrars).ToList(),
            implementationRegistrars
        );
    }
}