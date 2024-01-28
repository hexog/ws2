using System.Reflection;
using Ws2.DependencyInjection;
using Ws2.DependencyInjection.Abstractions;
using Ws2.DependencyInjection.Implementation;
using Ws2.DependencyInjection.Registrars;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceAttributeServiceCollectionExtensions
{
    public static readonly IServiceRegistrar[] DefaultAttributeRegistrars =
    {
        new SingletonServiceAttributeRegistrar(),
        new ScopedServiceAttributeRegistrar(),
        new TransientServiceAttributeRegistrar()
    };

    public static IServiceCollection AddServicesByAttributesFromTypes(
        this IServiceCollection serviceCollection,
        IEnumerable<Type> types,
        IEnumerable<IServiceRegistrar> serviceRegistrars
    )
    {
        ServiceRegistrarImplementation.RegisterServices(serviceCollection, types, serviceRegistrars);
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
            DefaultAttributeRegistrars
        );
    }

    public static IServiceCollection AddServicesFromAssembly(
        this IServiceCollection serviceCollection,
        IEnumerable<Assembly> assembliesToAdd,
        IEnumerable<IServiceRegistrar> serviceRegistrars
    )
    {
        var types = assembliesToAdd.SelectMany(x => x.DefinedTypes);
        return serviceCollection.AddServicesByAttributesFromTypes(types, serviceRegistrars);
    }

    public static IServiceCollection AddServicesFromAssembly(
        this IServiceCollection serviceCollection,
        Assembly assembly
    )
    {
        return serviceCollection.AddServicesByAttributesFromTypes(
            assembly.DefinedTypes,
            DefaultAttributeRegistrars
        );
    }

    public static IServiceCollection AddServicesFromAssembly(
        this IServiceCollection serviceCollection,
        Assembly assembly,
        IEnumerable<IServiceRegistrar> serviceRegistrars
    )
    {
        return serviceCollection.AddServicesByAttributesFromTypes(
            assembly.DefinedTypes,
            DefaultAttributeRegistrars.Concat(serviceRegistrars).ToList()
        );
    }

    public static IServiceCollection AddServicesFromAssemblyWithoutDefaultRegistrars(
        this IServiceCollection serviceCollection,
        IEnumerable<Assembly> assembliesToAdd,
        IEnumerable<IServiceRegistrar> serviceRegistrars
    )
    {
        var types = assembliesToAdd.SelectMany(x => x.DefinedTypes);
        return serviceCollection.AddServicesByAttributesFromTypes(
            types,
            DefaultAttributeRegistrars.Concat(serviceRegistrars).ToList()
        );
    }
}