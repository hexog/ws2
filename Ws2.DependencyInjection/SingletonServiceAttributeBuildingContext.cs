using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Ws2.DependencyInjection;

internal class SingletonServiceAttributeBuildingContext
{
    private MethodInfo? getRequiredServiceMethodInfo;

    public MethodInfo GetRequiredServiceMethodInfo => getRequiredServiceMethodInfo ??= CreateGetRequiredServiceMethodInfo();

    private static MethodInfo CreateGetRequiredServiceMethodInfo()
    {
        var serviceProviderServiceExtensions = typeof(ServiceProviderServiceExtensions);
        var getRequiredServiceMethod = serviceProviderServiceExtensions.GetMethod(
            "GetRequiredService",
            new[] { typeof(IServiceProvider), typeof(Type) }
        );
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
                    Expression.Constant(implementation)
                ),
                implementation
            ),
            serviceProviderParameterExpression
        );

        var factoryFunc = (Func<IServiceProvider, object>)factoryExpression.Compile();

        singletonInstanceFactoryCache[implementation] = factoryFunc;
        return factoryFunc;
    }
}