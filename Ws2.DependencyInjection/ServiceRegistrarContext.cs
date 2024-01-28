using Microsoft.Extensions.DependencyInjection;
using Ws2.DependencyInjection.Abstractions;

namespace Ws2.DependencyInjection;

public class ServiceRegistrarContext : IServiceRegistrarContext
{
    private readonly Dictionary<string, List<Type>> nameToType = new();

    private readonly Dictionary<string, List<Type>> fullNameToType = new();

    private readonly Dictionary<Type, List<IServiceRegistrar>> typeToServiceRegistrar = new();

    public ServiceRegistrarContext(
        IServiceCollection serviceCollection,
        List<Type> types,
        List<IServiceRegistrar> serviceRegistrars
    )
    {
        ServiceCollection = serviceCollection;
        Types = types;
        ServiceRegistrars = serviceRegistrars;

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

    IReadOnlyCollection<Type> IServiceRegistrarContext.Types => Types;

    public List<Type> Types { get; }

    IReadOnlyCollection<IServiceRegistrar> IServiceRegistrarContext.ServiceRegistrars => ServiceRegistrars;

    public List<IServiceRegistrar> ServiceRegistrars { get; }

    public Type? FindType(string? typeName)
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

    public bool IsValidImplementationType(Type implementationType)
    {
        // https://github.com/dotnet/runtime/blob/release/8.0/src/libraries/Microsoft.Extensions.DependencyInjection/src/ServiceLookup/CallSiteFactory.cs#L74
        return !(implementationType.IsAbstract || implementationType.IsInterface || implementationType.IsGenericTypeDefinition);
    }
}