namespace Ws2.Async.Locks.PooledLocks;

public abstract class SemaphoreLockHolder(ISemaphore semaphore) : ILockHolder
{
    protected readonly ISemaphore Semaphore = semaphore;
    private int isReleased;

    public abstract Task AcquireAsync();

    public async Task ReleaseAsync(CancellationToken cancellationToken = default)
    {
        if (Interlocked.Exchange(ref isReleased, 1) == 0)
        {
            await Semaphore.ReleaseAsync(cancellationToken).ConfigureAwait(false);
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            ReleaseAsync().GetAwaiter().GetResult();
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual async ValueTask DisposeAsyncCore()
    {
        await ReleaseAsync().ConfigureAwait(false);
    }

    public async ValueTask DisposeAsync()
    {
        await DisposeAsyncCore().ConfigureAwait(false);
        GC.SuppressFinalize(this);
    }
}