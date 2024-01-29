namespace Ws2.Async.Locks.PooledLocks;

public class SemaphoreSlimWrapper : ISemaphore
{
    private readonly SemaphoreSlim semaphore;

    public SemaphoreSlimWrapper(SemaphoreSlim semaphore)
    {
        this.semaphore = semaphore;
    }

    public async Task WaitAsync(CancellationToken cancellationToken)
    {
        await semaphore.WaitAsync(cancellationToken);
    }

    public async Task WaitAsync(TimeSpan timeSpan)
    {
        await semaphore.WaitAsync(timeSpan);
    }

    public void Release()
    {
        semaphore.Release();
    }

    public Task ReleaseAsync(CancellationToken cancellationToken)
    {
        semaphore.Release();
        return Task.CompletedTask;
    }

    public ValueTask DisposeAsync()
    {
        semaphore.Dispose();
        GC.SuppressFinalize(this);
        return ValueTask.CompletedTask;
    }
}