using JetBrains.Annotations;

namespace Ws2.Async.Locks.PooledLocks;

public class PooledSemaphoreLock<TKey> : ILock<TKey>
    where TKey : notnull
{
    private readonly ISemaphorePool semaphorePool;
    private readonly IEqualityComparer<TKey> equalityComparer;

    public PooledSemaphoreLock(ISemaphorePool semaphorePool, IEqualityComparer<TKey> equalityComparer)
    {
        this.semaphorePool = semaphorePool;
        this.equalityComparer = equalityComparer;
    }

    [MustUseReturnValue]
    public async ValueTask<ILockHolder> AcquireAsync(
        TKey key,
        TimeSpan timeout
    )
    {
        var hashCode = equalityComparer.GetHashCode(key);
        var semaphore = semaphorePool.GetSemaphore(hashCode);
        return await AcquireAsync(new SemaphoreTimeoutLockHolder(semaphore, timeout));
    }

    [MustUseReturnValue]
    public async ValueTask<ILockHolder> AcquireAsync(
        TKey key,
        CancellationToken cancellationToken
    )
    {
        var hashCode = equalityComparer.GetHashCode(key);
        var semaphore = semaphorePool.GetSemaphore(hashCode);
        return await AcquireAsync(new SemaphoreCancellableLockHolder(semaphore, cancellationToken));
    }

    private static async ValueTask<ILockHolder> AcquireAsync(SemaphoreLockHolder lockHolder)
    {
        await lockHolder.AcquireAsync();
        return lockHolder;
    }

    public async ValueTask DisposeAsync()
    {
        await semaphorePool.DisposeAsync();
        GC.SuppressFinalize(this);
    }
}