using Ws2.Async.Locks;
using Ws2.Async.Locks.PooledLocks;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class LockServiceCollectionsExtensions
{
    public static IServiceCollection AddSemaphoreLockProvider(
        this IServiceCollection services,
        object? lockKey
    )
    {
        services.AddKeyedSingleton<ILockProvider>(
            lockKey,
            (_, _) => new PooledSemaphoreLockProvider(new SemaphoreSlimPool())
        );

        return services;
    }

    public static IServiceCollection AddLockProvider(
        this IServiceCollection services,
        object? lockKey,
        ILockProvider lockInstance
    )
    {
        services.AddKeyedSingleton<ILockProvider>(
            lockKey,
            (_, _) => lockInstance
        );

        return services;
    }

    public static IServiceCollection AddLockProvider(
        this IServiceCollection services,
        object? lockKey,
        Func<IServiceProvider, object?, ILockProvider> lockFactory
    )
    {
        services.AddKeyedSingleton(
            lockKey,
            lockFactory
        );

        return services;
    }
}