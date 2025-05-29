namespace Ws2.Async.Locks.PooledLocks;

public interface ISemaphorePool : IDisposable, IAsyncDisposable
{
    ISemaphore GetSemaphore(int key);
}