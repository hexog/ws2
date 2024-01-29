using System.Collections.Concurrent;

namespace Ws2.Async.Locks.PooledLocks;

public class SemaphoreSlimPool : ISemaphorePool
{
    private readonly ConcurrentDictionary<int, SemaphoreSlimWrapper> semaphores = new();

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

    public ISemaphore GetSemaphore(int key)
    {
        return semaphores.GetOrAdd(key % size, static _ => new SemaphoreSlimWrapper(new SemaphoreSlim(1, 1)));
    }

    public async ValueTask DisposeAsync()
    {
        foreach (var semaphorePoolEntry in semaphores.Values)
        {
            await semaphorePoolEntry.DisposeAsync();
        }

        GC.SuppressFinalize(this);
    }
}