using Microsoft.Extensions.DependencyInjection;

namespace Ws2.DependencyInjection.Abstractions;

public class ServiceAttributeRegistrarContext
{
    private readonly Dictionary<Type, IServiceAttributeRegistrar?> serviceAttributeRegistrarCache = new();

    private readonly Dictionary<Type, IServiceTypeImplementationRegistrar?> serviceTypeImplementationRegistrarCache =
        new();

    public ServiceAttributeRegistrarContext(
        IServiceCollection serviceCollection,
        IReadOnlyCollection<Type> types,
        ILookup<string, Type> nameToType,
        IReadOnlyDictionary<string, Type> fullNameToType,
        IReadOnlyCollection<IServiceAttributeRegistrar> serviceAttributeRegistrar,
        IReadOnlyCollection<IServiceTypeImplementationRegistrar> serviceTypeImplementationRegistrars
    )
    {
        ServiceCollection = serviceCollection;
        Types = types;
        NameToType = nameToType;
        FullNameToType = fullNameToType;
        ServiceAttributeRegistrar = serviceAttributeRegistrar;
        ServiceTypeImplementationRegistrars = serviceTypeImplementationRegistrars;
    }

    public IServiceCollection ServiceCollection { get; }

    public IReadOnlyCollection<Type> Types { get; }

    public ILookup<string, Type> NameToType { get; }

    public IReadOnlyDictionary<string, Type> FullNameToType { get; }

    public IReadOnlyCollection<IServiceAttributeRegistrar> ServiceAttributeRegistrar { get; }

    public IReadOnlyCollection<IServiceTypeImplementationRegistrar> ServiceTypeImplementationRegistrars { get; }

    public IServiceAttributeRegistrar? FindServiceAttributeRegistrar(Type serviceAttributeType)
    {
        if (serviceAttributeRegistrarCache.TryGetValue(serviceAttributeType, out var registrar))
        {
            return registrar;
        }

        registrar = ServiceAttributeRegistrar
            .FirstOrDefault(x => x.ServiceAttributeType.IsAssignableFrom(serviceAttributeType));
        serviceAttributeRegistrarCache[serviceAttributeType] = registrar;
        return registrar;
    }

    public IServiceTypeImplementationRegistrar? FindServiceImplementationRegistrar(Type serviceType)
    {
        if (serviceTypeImplementationRegistrarCache.TryGetValue(serviceType, out var registrar))
        {
            return registrar;
        }

        registrar = ServiceTypeImplementationRegistrars
            .FirstOrDefault(x => x.ServiceType.IsAssignableFrom(serviceType));
        serviceTypeImplementationRegistrarCache[serviceType] = registrar;
        return registrar;
    }
}