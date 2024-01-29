namespace Ws2.Async.Locks.PooledLocks;

public abstract class SemaphoreLockHolder : ILockHolder
{
    protected readonly ISemaphore Semaphore;
    private int isDisposed;

    protected SemaphoreLockHolder(ISemaphore semaphore)
    {
        Semaphore = semaphore;
    }

    public abstract Task AcquireAsync();

    public async Task ReleaseAsync(CancellationToken cancellationToken = default)
    {
        if (Interlocked.Exchange(ref isDisposed, 1) == 0)
        {
            await Semaphore.ReleaseAsync(cancellationToken);
        }
    }

    public async ValueTask DisposeAsync()
    {
        await ReleaseAsync();
        GC.SuppressFinalize(this);
    }
}