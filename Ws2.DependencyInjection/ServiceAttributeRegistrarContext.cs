using Microsoft.Extensions.DependencyInjection;
using Ws2.DependencyInjection.Abstractions;

namespace Ws2.DependencyInjection;

public class ServiceAttributeRegistrarContext : IServiceAttributeRegistrarContext
{
    private readonly Dictionary<string, List<Type>> nameToType = new();

    private readonly Dictionary<string, List<Type>> fullNameToType = new();

    private readonly Dictionary<Type, List<IServiceAttributeRegistrar>> serviceAttributeTypeToRegistrar = new();

    private readonly Dictionary<Type, List<IServiceTypeImplementationRegistrar>> serviceTypeImplementationToRegistrar =
        new();

    public ServiceAttributeRegistrarContext(
        IServiceCollection serviceCollection,
        List<Type> types,
        IReadOnlyCollection<IServiceAttributeRegistrar> attributeRegistrars,
        IReadOnlyCollection<IServiceTypeImplementationRegistrar> implementationRegistrars
    )
    {
        ServiceCollection = serviceCollection;
        Types = types;

        ServiceAttributeRegistrar = attributeRegistrars;
        ServiceTypeImplementationRegistrars = implementationRegistrars;

        AnalyzeTypes(types);
    }

    private void AnalyzeTypes(List<Type> types)
    {
        foreach (var type in types)
        {
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

    public IReadOnlyCollection<IServiceAttributeRegistrar> ServiceAttributeRegistrar { get; }

    public IReadOnlyCollection<IServiceTypeImplementationRegistrar> ServiceTypeImplementationRegistrars { get; }

    public Type? FIndType(string? typeName)
    {
        if (typeName is null)
        {
            return null;
        }

        if (fullNameToType.TryGetValue(typeName, out var services))
        {
            if (services.Count > 1
                && ServiceAttributeCollectionPopulationOptions.Global.ThrowOnMultipleTypesWithSameFullName)
            {
                AttributeServiceCollectionPopulationException
                    .ThrowMultipleTypesWithSameFullNameExist(services.First().FullName!);
            }

            if (services.SingleOrDefault() is { } service)
            {
                return service;
            }
        }

        var types = nameToType[typeName];
        if (types.Count > 1 && ServiceAttributeCollectionPopulationOptions.Global.ThrowOnMultipleTypesWithSameName)
        {
            AttributeServiceCollectionPopulationException.ThrowMultipleTypesWithSameNameExist(types.First().Name);
        }

        return types.SingleOrDefault();
    }

    IEnumerable<IServiceAttributeRegistrar> IServiceAttributeRegistrarContext.FindServiceAttributeRegistrars(
        Type serviceAttributeType
    )
    {
        return FindServiceAttributeRegistrars(serviceAttributeType);
    }

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
        FindServiceImplementationRegistrars(Type serviceType)
    {
        return FindServiceImplementationRegistrars(serviceType);
    }

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