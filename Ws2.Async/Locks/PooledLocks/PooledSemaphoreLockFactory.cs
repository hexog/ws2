using JetBrains.Annotations;
using Ws2.EqualityComparison.ByteMemory;

namespace Ws2.Async.Locks.PooledLocks;

public class PooledSemaphoreLockFactory : ILockFactory
{
    private readonly ISemaphorePool semaphorePool;
    private readonly EqualityComparer<ReadOnlyMemory<byte>> keyComparer;

    public PooledSemaphoreLockFactory(ISemaphorePool semaphorePool)
        : this(semaphorePool, ReadOnlyByteMemoryEqualityComparer.Instance)
    {
    }

    public PooledSemaphoreLockFactory(ISemaphorePool semaphorePool, EqualityComparer<ReadOnlyMemory<byte>> keyComparer)
    {
        this.semaphorePool = semaphorePool;
        this.keyComparer = keyComparer;
    }

    [MustUseReturnValue]
    [MustDisposeResource]
    public virtual async ValueTask<ILockHolder> AcquireAsync(ReadOnlyMemory<byte> key, TimeSpan timeout)
    {
        var hashCode = keyComparer.GetHashCode(key);
        var semaphore = semaphorePool.GetSemaphore(hashCode);
        return await AcquireAsync(new SemaphoreTimeoutLockHolder(semaphore, timeout));
    }

    [MustUseReturnValue]
    [MustDisposeResource]
    public virtual async ValueTask<ILockHolder> AcquireAsync(ReadOnlyMemory<byte> key, CancellationToken cancellationToken)
    {
        var hashCode = keyComparer.GetHashCode(key);
        var semaphore = semaphorePool.GetSemaphore(hashCode);
        return await AcquireAsync(new SemaphoreCancellableLockHolder(semaphore, cancellationToken));
    }

    private static async ValueTask<ILockHolder> AcquireAsync(SemaphoreLockHolder lockHolder)
    {
        await lockHolder.AcquireAsync();
        return lockHolder;
    }

    #region Dispose

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            semaphorePool.Dispose();
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual async ValueTask DisposeAsyncCore()
    {
        await semaphorePool.DisposeAsync();
    }

    public async ValueTask DisposeAsync()
    {
        await DisposeAsyncCore();
        GC.SuppressFinalize(this);
    }

    #endregion
}