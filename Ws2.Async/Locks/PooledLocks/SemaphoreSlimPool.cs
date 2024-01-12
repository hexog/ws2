using System.Collections.Concurrent;

namespace Ws2.Async.Locks.PooledLocks;

public class SemaphoreSlimPool : ISemaphorePool, IDisposable
{
	private readonly record struct SemaphorePoolEntry(SemaphoreSlim Semaphore);

	private readonly ConcurrentDictionary<int, SemaphorePoolEntry> semaphores = new();

	private readonly int size = 0b_0011_1111; // 128

	private const int MaxPoolSize = 0xffff; // 65_535

	public int Size
	{
		get => size;
		init
		{
			ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(value, 0);
			ArgumentOutOfRangeException.ThrowIfGreaterThan(value, MaxPoolSize);
			size = value;
		}
	}

	public SemaphoreSlim GetSemaphore(int key)
	{
		var entry = semaphores.GetOrAdd(key % size, static _ => new SemaphorePoolEntry(new SemaphoreSlim(1, 1)));
		return entry.Semaphore;
	}

	public void Dispose()
	{
		foreach (var semaphorePoolEntry in semaphores.Values)
		{
			semaphorePoolEntry.Semaphore.Dispose();
		}

		GC.SuppressFinalize(this);
	}
}