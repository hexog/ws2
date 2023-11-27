using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Ws2.DependencyInjection;

internal class ServiceAttributeBuildingContext
{
    private readonly IReadOnlyList<Type> types;
    private readonly ILookup<string, Type> typesByName;
    private readonly Dictionary<string, Type> typesByFullName;

    private ServiceAttributeBuildingContext(
        IReadOnlyList<Type> types,
        ILookup<string, Type> typesByName,
        Dictionary<string, Type> typesByFullName
    )
    {
        this.typesByName = typesByName;
        this.typesByFullName = typesByFullName;
        this.types = types;
    }

    public static ServiceAttributeBuildingContext FromTypes(IEnumerable<Type> types)
    {
        var typesCopy = types.ToList();

        return new ServiceAttributeBuildingContext(
            typesCopy,
            typesCopy.ToLookup(x => x.Name),
            typesCopy
                .Select(x => (Type: x, x.FullName))
                .Where(x => x.FullName is not null)
                .ToDictionary(x => x.FullName!, x => x.Type)
        );
    }

    public IEnumerable<Type> Types => types;

    public Type? FindServiceType(ServiceAttribute serviceAttribute)
    {
        if (serviceAttribute.Service is not null)
        {
            return serviceAttribute.Service;
        }

        return FindTypeByName(serviceAttribute.ServiceTypeName);
    }

    public Type? FindTypeByName(string? typeNameOrFullyQualifiedTypeName)
    {
        if (typeNameOrFullyQualifiedTypeName is null)
        {
            return null;
        }

        if (typesByFullName.TryGetValue(typeNameOrFullyQualifiedTypeName, out var type))
        {
            return type;
        }

        var typesWithName = typesByName[typeNameOrFullyQualifiedTypeName].Take(2).ToArray();
        if (typesByName.Count > 1)
        {
            throw new ArgumentException($"Multiple types found with name {typeNameOrFullyQualifiedTypeName}");
        }

        if (typesWithName.Length == 0)
        {
            return typesWithName[0];
        }

        return null;
    }

    #region Singleton service creation

    private MethodInfo? getRequiredServiceMethodInfo;

    public MethodInfo GetRequiredServiceMethodInfo =>
        getRequiredServiceMethodInfo ??= CreateGetRequiredServiceMethodInfo();

    private static MethodInfo CreateGetRequiredServiceMethodInfo()
    {
        var serviceProviderServiceExtensions = typeof(ServiceProviderServiceExtensions);
        var getRequiredServiceMethod = serviceProviderServiceExtensions.GetMethod("GetRequiredService",
            new[] { typeof(IServiceProvider), typeof(Type) });
        Debug.Assert(getRequiredServiceMethod is not null);
        return getRequiredServiceMethod;
    }

    private readonly Dictionary<Type, Type> factoryDelegateTypeCache = new();

    public Type GetFactoryDelegateType(Type implementation)
    {
        if (factoryDelegateTypeCache.TryGetValue(implementation, out var factoryDelegateType))
        {
            return factoryDelegateType;
        }

        var factoryType = typeof(Func<,>);
        var genericFactoryType = factoryType.MakeGenericType(typeof(IServiceProvider), implementation);
        factoryDelegateTypeCache.Add(implementation, genericFactoryType);
        return genericFactoryType;
    }

    private readonly Dictionary<Type, Func<IServiceProvider, object>> singletonInstanceFactoryCache = new();

    public Func<IServiceProvider, object> GetSingletonInstanceFactory(Type implementation)
    {
        if (singletonInstanceFactoryCache.TryGetValue(implementation, out var singletonInstanceFactory))
        {
            return singletonInstanceFactory;
        }

        var genericFactoryType = GetFactoryDelegateType(implementation);

        var serviceProviderParameterExpression = Expression.Parameter(typeof(IServiceProvider), "p");
        var factoryExpression = Expression.Lambda(
            genericFactoryType,
            Expression.ConvertChecked(
                Expression.Call(
                    null,
                    GetRequiredServiceMethodInfo,
                    serviceProviderParameterExpression,
                    Expression.Constant(implementation)),
                implementation
            ),
            serviceProviderParameterExpression
        );

        var factoryFunc = (Func<IServiceProvider, object>)factoryExpression.Compile();

        singletonInstanceFactoryCache[implementation] = factoryFunc;
        return factoryFunc;
    }

    #endregion
}