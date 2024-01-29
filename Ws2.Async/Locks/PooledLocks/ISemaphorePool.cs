namespace Ws2.Async.Locks.PooledLocks;

public interface ISemaphorePool : IAsyncDisposable
{
    ISemaphore GetSemaphore(int key);
}