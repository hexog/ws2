namespace Ws2.Async.Locks.PooledLocks;

public sealed class SemaphoreSlimWrapper(SemaphoreSlim semaphore) : ISemaphore
{
    public async Task WaitAsync(CancellationToken cancellationToken)
    {
        await semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task WaitAsync(TimeSpan timeSpan)
    {
        await semaphore.WaitAsync(timeSpan).ConfigureAwait(false);
    }

    public Task ReleaseAsync(CancellationToken cancellationToken)
    {
        semaphore.Release();
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        semaphore.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        // ReSharper disable once SuspiciousTypeConversion.Global
        if (semaphore is IAsyncDisposable semaphoreAsyncDisposable)
        {
            await semaphoreAsyncDisposable.DisposeAsync().ConfigureAwait(false);
        }
        else
        {
            semaphore.Dispose();
        }
    }
}