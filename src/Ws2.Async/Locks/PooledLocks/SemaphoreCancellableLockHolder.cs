namespace Ws2.Async.Locks.PooledLocks;

public class SemaphoreCancellableLockHolder(ISemaphore semaphore, CancellationToken cancellationToken)
    : SemaphoreLockHolder(semaphore)
{
    public override async Task AcquireAsync()
    {
        await semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
    }
}