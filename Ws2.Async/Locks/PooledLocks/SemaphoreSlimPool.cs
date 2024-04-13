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

    protected virtual void Dispose(bool disposing)
    {
        foreach (var value in semaphores.Values)
        {
            value.Dispose();
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual async ValueTask DisposeAsyncCore()
    {
        foreach (var value in semaphores.Values)
        {
            await value.DisposeAsync();
        }
    }

    public async ValueTask DisposeAsync()
    {
        await DisposeAsyncCore();
        GC.SuppressFinalize(this);
    }
}