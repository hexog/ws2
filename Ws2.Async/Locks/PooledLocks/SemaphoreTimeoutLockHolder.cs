namespace Ws2.Async.Locks.PooledLocks;

public class SemaphoreTimeoutLockHolder : SemaphoreLockHolder
{
    private readonly TimeSpan timeout;

    public SemaphoreTimeoutLockHolder(ISemaphore semaphore, TimeSpan timeout) : base(semaphore)
    {
        this.timeout = timeout;
    }

    public override async Task AcquireAsync()
    {
        await Semaphore.WaitAsync(timeout).ConfigureAwait(false);
    }
}