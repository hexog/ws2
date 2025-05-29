namespace Ws2.Async.Locks.PooledLocks;

public class SemaphoreTimeoutLockHolder(ISemaphore semaphore, TimeSpan timeout) : SemaphoreLockHolder(semaphore)
{
    public override async Task AcquireAsync()
    {
        await semaphore.WaitAsync(timeout).ConfigureAwait(false);
    }
}