using System.Diagnostics;

namespace Ws2.Async.Locks.PooledLocks;

public class SemaphoreLockHolder : ILockHolder
{
    private readonly SemaphoreSlim semaphore;
    private readonly CancellationToken? cancellationToken;
    private readonly TimeSpan? timeout;

    public SemaphoreLockHolder(SemaphoreSlim semaphore, CancellationToken cancellationToken)
    {
        this.semaphore = semaphore;
        this.cancellationToken = cancellationToken;
    }

    public SemaphoreLockHolder(SemaphoreSlim semaphore, TimeSpan timeout)
    {
        this.semaphore = semaphore;
        this.timeout = timeout;
    }

    public async Task Acquire()
    {
        if (cancellationToken is { } cancellationTokenValue)
        {
            await semaphore.WaitAsync(cancellationTokenValue);
            return;
        }

        Debug.Assert(timeout.HasValue);
        await semaphore.WaitAsync(timeout.Value);
    }

    public void Dispose()
    {
        semaphore.Release();
        GC.SuppressFinalize(this);
    }
}