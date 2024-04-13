using Ws2.Async.Locks;
using Ws2.Async.Locks.PooledLocks;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class LockServiceCollectionsExtensions
{
    public static IServiceCollection AddLockFactory(
        this IServiceCollection services,
        object? lockKey
    )
    {
        services.AddKeyedSingleton<ILockFactory>(
            lockKey,
            (_, _) => new PooledSemaphoreLockFactory(new SemaphoreSlimPool())
        );

        return services;
    }

    public static IServiceCollection AddLockFactory(
        this IServiceCollection services,
        object? lockKey,
        ILockFactory lockInstance
    )
    {
        services.AddKeyedSingleton<ILockFactory>(
            lockKey,
            (_, _) => lockInstance
        );

        return services;
    }

    public static IServiceCollection AddLockFactory(
        this IServiceCollection services,
        object? lockKey,
        Func<IServiceProvider, object?, ILockFactory> lockFactory
    )
    {
        services.AddKeyedSingleton(
            lockKey,
            lockFactory
        );

        return services;
    }
}