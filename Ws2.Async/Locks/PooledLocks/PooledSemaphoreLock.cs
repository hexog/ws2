using JetBrains.Annotations;

namespace Ws2.Async.Locks.PooledLocks;

public class PooledSemaphoreLock : ILock
{
    protected readonly ISemaphorePool SemaphorePool;

    public PooledSemaphoreLock(ISemaphorePool semaphorePool)
    {
        SemaphorePool = semaphorePool;
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
        var semaphore = SemaphorePool.GetSemaphore(hashCode);
        return await AcquireAsync(new SemaphoreTimeoutLockHolder(semaphore, timeout));
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
        var semaphore = SemaphorePool.GetSemaphore(hashCode);
        return await AcquireAsync(new SemaphoreCancellableLockHolder(semaphore, cancellationToken));
    }

    private static async ValueTask<ILockHolder> AcquireAsync(SemaphoreLockHolder lockHolder)
    {
        await lockHolder.AcquireAsync();
        return lockHolder;
    }

    public async ValueTask DisposeAsync()
    {
        await SemaphorePool.DisposeAsync();
        GC.SuppressFinalize(this);
    }
}

public class PooledSemaphoreLock<TKey> : PooledSemaphoreLock, ILock<TKey>
    where TKey : notnull
{
    protected readonly IEqualityComparer<TKey> EqualityComparer;

    public PooledSemaphoreLock(ISemaphorePool semaphorePool, IEqualityComparer<TKey> equalityComparer) : base(semaphorePool)
    {
        EqualityComparer = equalityComparer;
    }

    public async ValueTask<ILockHolder> AcquireAsync(TKey key, TimeSpan timeout)
    {
        return await AcquireAsync(key, EqualityComparer, timeout);
    }

    public async ValueTask<ILockHolder> AcquireAsync(TKey key, CancellationToken cancellationToken)
    {
        return await AcquireAsync(key, EqualityComparer, cancellationToken);
    }
}