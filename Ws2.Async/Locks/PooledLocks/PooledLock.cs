using JetBrains.Annotations;

namespace Ws2.Async.Locks.PooledLocks;

public class PooledLock : ILock
{
    private readonly ISemaphorePool semaphorePool;

    public PooledLock(ISemaphorePool semaphorePool)
    {
        this.semaphorePool = semaphorePool;
    }

    [MustUseReturnValue]
    public async ValueTask<ILockHolder> AcquireAsync<TKey>(
        TKey key,
        IEqualityComparer<TKey> comparer,
        TimeSpan timeout
    )
        where TKey : notnull
    {
        var hashCode = comparer.GetHashCode(key);
        var semaphore = semaphorePool.GetSemaphore(hashCode);
        var lockHolder = new SemaphoreLockHolder(semaphore, timeout);
        await lockHolder.Acquire();
        return lockHolder;
    }

    [MustUseReturnValue]
    public async ValueTask<ILockHolder> AcquireAsync<TKey>(
        TKey key,
        IEqualityComparer<TKey> comparer,
        CancellationToken cancellationToken
    )
        where TKey : notnull
    {
        var hashCode = comparer.GetHashCode(key);
        var semaphore = semaphorePool.GetSemaphore(hashCode);
        var lockHolder = new SemaphoreLockHolder(semaphore, cancellationToken);
        await lockHolder.Acquire();
        return lockHolder;
    }
}