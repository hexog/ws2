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
}