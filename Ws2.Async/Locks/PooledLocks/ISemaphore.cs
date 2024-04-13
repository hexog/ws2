namespace Ws2.Async.Locks.PooledLocks;

public interface ISemaphore : IDisposable, IAsyncDisposable
{
    Task WaitAsync(CancellationToken cancellationToken);

    Task WaitAsync(TimeSpan timeSpan);

    Task ReleaseAsync(CancellationToken cancellationToken);
}