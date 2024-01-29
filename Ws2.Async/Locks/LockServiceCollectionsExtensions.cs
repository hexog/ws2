using Ws2.Async.Locks;
using Ws2.Async.Locks.PooledLocks;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class LockServiceCollectionsExtensions
{
    public static IServiceCollection AddLock(this IServiceCollection services, object? lockKey)
    {
        services.AddKeyedSingleton<ILock>(
            lockKey,
            static (_, _) => new PooledLock(new SemaphoreSlimPool())
        );

        return services;
    }

    public static IServiceCollection AddLock(this IServiceCollection services, object? lockKey, ILock lockInstance)
    {
        services.AddKeyedSingleton<ILock>(
            lockKey,
            (_, _) => lockInstance
        );

        return services;
    }

    public static IServiceCollection AddLock(
        this IServiceCollection services,
        object? lockKey,
        Func<IServiceProvider, object?, ILock> lockFactory
    )
    {
        services.AddKeyedSingleton(
            lockKey,
            lockFactory
        );

        return services;
    }
}