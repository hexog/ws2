namespace Ws2.Async.Locks.PooledLocks;

public interface ISemaphorePool
{
	SemaphoreSlim GetSemaphore(int key);
}