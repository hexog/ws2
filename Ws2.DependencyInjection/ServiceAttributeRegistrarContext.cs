using Microsoft.Extensions.DependencyInjection;
using Ws2.DependencyInjection.Abstractions;

namespace Ws2.DependencyInjection;

public class ServiceAttributeRegistrarContext : IServiceAttributeRegistrarContext
{
    private readonly Dictionary<string, List<Type>> nameToType = new();

    private readonly Dictionary<string, List<Type>> fullNameToType = new();

    private readonly List<IServiceAttributeRegistrar> serviceAttributeRegistrars = new();

    private readonly List<IServiceTypeImplementationRegistrar> serviceTypeImplementationRegistrars = new();

    private readonly Dictionary<Type, List<IServiceAttributeRegistrar>> serviceAttributeTypeToRegistrar = new();

    private readonly Dictionary<Type, List<IServiceTypeImplementationRegistrar>> serviceTypeImplementationToRegistrar =
        new();

    public ServiceAttributeRegistrarContext(
        IServiceCollection serviceCollection,
        List<Type> types
    )
    {
        ServiceCollection = serviceCollection;
        Types = types;

        AnalyzeTypes(types);
    }

    private void AnalyzeTypes(List<Type> types)
    {
        foreach (var type in types)
        {
            if (type.IsAssignableTo(typeof(IServiceAttributeRegistrar)))
            {
                var serviceAttributeRegistrar = Activator.CreateInstance<IServiceAttributeRegistrar>();
                serviceAttributeRegistrars.Add(serviceAttributeRegistrar);
            }

            if (type.IsAssignableTo(typeof(IServiceTypeImplementationRegistrar)))
            {
                var serviceTypeImplementationRegistrar =
                    Activator.CreateInstance<IServiceTypeImplementationRegistrar>();
                serviceTypeImplementationRegistrars.Add(serviceTypeImplementationRegistrar);
            }

            if (nameToType.TryGetValue(type.Name, out var typeNameList))
            {
                typeNameList.Add(type);
            }
            else
            {
                nameToType[type.Name] = new List<Type> { type };
            }

            if (type.FullName is { } typeFullName)
            {
                if (fullNameToType.TryGetValue(typeFullName, out var list))
                {
                    list.Add(type);
                }
                else
                {
                    fullNameToType[typeFullName] = new List<Type> { type };
                }
            }
        }
    }

    public IServiceCollection ServiceCollection { get; }

    IReadOnlyCollection<Type> IServiceAttributeRegistrarContext.Types => Types;

    public List<Type> Types { get; }

    public IReadOnlyCollection<IServiceAttributeRegistrar> ServiceAttributeRegistrar =>
        serviceAttributeRegistrars;

    public IReadOnlyCollection<IServiceTypeImplementationRegistrar> ServiceTypeImplementationRegistrars =>
        serviceTypeImplementationRegistrars;

    public Type? FIndType(string? typeName)
    {
        if (typeName is null)
        {
            return null;
        }

        if (fullNameToType.TryGetValue(typeName, out var services))
        {
            if (services.SingleOrDefault() is { } service)
            {
                return service;
            }
        }

        var types = nameToType[typeName];
        return types.SingleOrDefault();
    }

    IEnumerable<IServiceAttributeRegistrar> IServiceAttributeRegistrarContext.FindServiceAttributeRegistrars(
        Type serviceAttributeType
    ) => FindServiceAttributeRegistrars(serviceAttributeType);

    public List<IServiceAttributeRegistrar> FindServiceAttributeRegistrars(Type serviceAttributeType)
    {
        if (serviceAttributeTypeToRegistrar.TryGetValue(serviceAttributeType, out var registrars))
        {
            return registrars;
        }

        registrars = ServiceAttributeRegistrar
            .Where(x => x.ServiceAttributeType.IsAssignableFrom(serviceAttributeType))
            .ToList();

        return serviceAttributeTypeToRegistrar[serviceAttributeType] = registrars;
    }

    IEnumerable<IServiceTypeImplementationRegistrar> IServiceAttributeRegistrarContext.
        FindServiceImplementationRegistrars(Type serviceType) => FindServiceImplementationRegistrars(serviceType);

    public List<IServiceTypeImplementationRegistrar> FindServiceImplementationRegistrars(Type serviceType)
    {
        if (serviceTypeImplementationToRegistrar.TryGetValue(serviceType, out var registrars))
        {
            return registrars;
        }

        registrars = ServiceTypeImplementationRegistrars
            .Where(x => x.ServiceType.IsAssignableFrom(serviceType))
            .ToList();

        return serviceTypeImplementationToRegistrar[serviceType] = registrars;
    }
}