namespace Ws2.Async.Locks.PooledLocks;

public class SemaphoreCancellableLockHolder : SemaphoreLockHolder
{
    private readonly CancellationToken cancellationToken;

    public SemaphoreCancellableLockHolder(ISemaphore semaphore, CancellationToken cancellationToken) : base(semaphore)
    {
        this.cancellationToken = cancellationToken;
    }

    public override async Task AcquireAsync()
    {
        await Semaphore.WaitAsync(cancellationToken);
    }
}