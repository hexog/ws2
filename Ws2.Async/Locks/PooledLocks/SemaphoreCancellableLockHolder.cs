namespace Ws2.Async.Locks.PooledLocks;

public class SemaphoreCancellableLockHolder(ISemaphore semaphore, CancellationToken cancellationToken)
    : SemaphoreLockHolder(semaphore)
{
    public override async Task AcquireAsync()
    {
        await Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
    }
}