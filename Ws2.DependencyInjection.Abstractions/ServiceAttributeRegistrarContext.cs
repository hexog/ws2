using Microsoft.Extensions.DependencyInjection;

namespace Ws2.DependencyInjection.Abstractions;

public class ServiceAttributeRegistrarContext
{
    private readonly Dictionary<Type, IServiceAttributeRegistrar?> serviceAttributeRegistrarByTypeCache = new();

    public ServiceAttributeRegistrarContext(
        IServiceCollection serviceCollection,
        IReadOnlyCollection<Type> types,
        ILookup<string, Type> nameToType,
        IReadOnlyDictionary<string, Type> fullNameToType,
        IReadOnlyCollection<IServiceAttributeRegistrar> serviceAttributeRegistrar
    )
    {
        ServiceCollection = serviceCollection;
        Types = types;
        NameToType = nameToType;
        FullNameToType = fullNameToType;
        ServiceAttributeRegistrar = serviceAttributeRegistrar;
    }

    public IServiceCollection ServiceCollection { get; }

    public IReadOnlyCollection<Type> Types { get; }

    public ILookup<string, Type> NameToType { get; }

    public IReadOnlyDictionary<string, Type> FullNameToType { get; }

    public IReadOnlyCollection<IServiceAttributeRegistrar> ServiceAttributeRegistrar { get; }

    public IServiceAttributeRegistrar? FindServiceAttributeRegistrar(Type serviceAttributeType)
    {
        if (serviceAttributeRegistrarByTypeCache.TryGetValue(serviceAttributeType, out var registrar))
        {
            return registrar;
        }

        registrar =  ServiceAttributeRegistrar
            .FirstOrDefault(x => x.ServiceAttributeType.IsAssignableFrom(serviceAttributeType));
        serviceAttributeRegistrarByTypeCache[serviceAttributeType] = registrar;
        return registrar;
    }
}