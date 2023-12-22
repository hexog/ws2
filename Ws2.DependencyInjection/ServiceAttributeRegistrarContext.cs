using Microsoft.Extensions.DependencyInjection;
using Ws2.DependencyInjection.Abstractions;

namespace Ws2.DependencyInjection;

public class ServiceAttributeRegistrarContext : IServiceAttributeRegistrarContext
{
    private readonly Dictionary<string, List<Type>> nameToType = new();

    private readonly Dictionary<string, List<Type>> fullNameToType = new();

    private readonly List<IServiceAttributeRegistrar> serviceAttributeRegistrars = new();

    private readonly List<IServiceTypeImplementationRegistrar> serviceTypeImplementationRegistrars = new();

    private readonly Dictionary<Type, IServiceAttributeRegistrar?> serviceAttributeTypeToRegistrar = new();

    private readonly Dictionary<Type, IServiceTypeImplementationRegistrar?> serviceTypeImplementationToRegistrar =
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

        if (fullNameToType.TryGetValue(typeName, out var service) && service.SingleOrDefault() is { } singleService)
        {
            return singleService;
        }

        var types = nameToType[typeName];
        return types.SingleOrDefault();
    }

    public IServiceAttributeRegistrar? FindServiceAttributeRegistrar(Type serviceAttributeType)
    {
        if (serviceAttributeTypeToRegistrar.TryGetValue(serviceAttributeType, out var registrar))
        {
            return registrar;
        }

        registrar = ServiceAttributeRegistrar.FirstOrDefault(
            x => x.ServiceAttributeType.IsAssignableFrom(serviceAttributeType)
        );

        return serviceAttributeTypeToRegistrar[serviceAttributeType] = registrar;
    }

    public IServiceTypeImplementationRegistrar? FindServiceImplementationRegistrar(Type serviceType)
    {
        if (serviceTypeImplementationToRegistrar.TryGetValue(serviceType, out var registrar))
        {
            return registrar;
        }

        registrar = ServiceTypeImplementationRegistrars.FirstOrDefault(
            x => x.ServiceType.IsAssignableFrom(serviceType)
        );

        return serviceTypeImplementationToRegistrar[serviceType] = registrar;
    }
}