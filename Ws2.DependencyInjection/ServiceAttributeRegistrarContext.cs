using Microsoft.Extensions.DependencyInjection;
using Ws2.DependencyInjection.Abstractions;

namespace Ws2.DependencyInjection;

public class ServiceAttributeRegistrarContext : IServiceAttributeRegistrarContext
{
    private readonly Lazy<ILookup<string, Type>> nameToType;

    private readonly Lazy<IReadOnlyDictionary<string, Type>> fullNameToType;

    private readonly Dictionary<Type, IServiceAttributeRegistrar?> serviceAttributeRegistrarCache = new();

    private readonly Dictionary<Type, IServiceTypeImplementationRegistrar?> serviceTypeImplementationRegistrarCache =
        new();

    public ServiceAttributeRegistrarContext(
        IServiceCollection serviceCollection,
        IReadOnlyCollection<Type> types,
        Lazy<ILookup<string, Type>> nameToType,
        Lazy<IReadOnlyDictionary<string, Type>> fullNameToType,
        IReadOnlyCollection<IServiceAttributeRegistrar> serviceAttributeRegistrar,
        IReadOnlyCollection<IServiceTypeImplementationRegistrar> serviceTypeImplementationRegistrars
    )
    {
        ServiceCollection = serviceCollection;
        Types = types;
        this.nameToType = nameToType;
        this.fullNameToType = fullNameToType;
        ServiceAttributeRegistrar = serviceAttributeRegistrar;
        ServiceTypeImplementationRegistrars = serviceTypeImplementationRegistrars;
    }

    public IServiceCollection ServiceCollection { get; }

    public IReadOnlyCollection<Type> Types { get; }

    public IReadOnlyCollection<IServiceAttributeRegistrar> ServiceAttributeRegistrar { get; }

    public IReadOnlyCollection<IServiceTypeImplementationRegistrar> ServiceTypeImplementationRegistrars { get; }

    public Type? FIndType(string? typeName)
    {
        if (typeName is null)
        {
            return null;
        }

        if (fullNameToType.Value.TryGetValue(typeName, out var service))
        {
            return service;
        }

        var types = nameToType.Value[typeName];
        return types.SingleOrDefault();
    }

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