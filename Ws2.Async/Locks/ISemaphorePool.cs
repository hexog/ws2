namespace Ws2.Async.Locks;

public interface ISemaphorePool
{
	SemaphoreSlim GetSemaphore(int key);
}