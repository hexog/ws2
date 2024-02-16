using Ws2.Async.Locks;
using Ws2.Async.Locks.PooledLocks;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class LockServiceCollectionsExtensions
{
    public static IServiceCollection AddLock<TLockKey>(
        this IServiceCollection services,
        object? lockKey
    ) where TLockKey : notnull
    {
        return AddLock(services, lockKey, EqualityComparer<TLockKey>.Default);
    }

    public static IServiceCollection AddLock<TLockKey>(
        this IServiceCollection services,
        object? lockKey,
        IEqualityComparer<TLockKey> equalityComparer
    ) where TLockKey : notnull
    {
        services.AddKeyedSingleton<ILock<TLockKey>>(
            lockKey,
            (_, _) => new PooledSemaphoreLock<TLockKey>(new SemaphoreSlimPool(), equalityComparer)
        );

        return services;
    }

    public static IServiceCollection AddLock<TLockKey>(
        this IServiceCollection services,
        object? lockKey,
        ILock<TLockKey> lockInstance
    ) where TLockKey : notnull
    {
        services.AddKeyedSingleton<ILock<TLockKey>>(
            lockKey,
            (_, _) => lockInstance
        );

        return services;
    }

    public static IServiceCollection AddLock<TLockKey>(
        this IServiceCollection services,
        object? lockKey,
        Func<IServiceProvider, object?, ILock<TLockKey>> lockFactory
    ) where TLockKey : notnull
    {
        services.AddKeyedSingleton(
            lockKey,
            lockFactory
        );

        return services;
    }
}