using FluentAssertions;
using Ws2.Async.Locks.PooledLocks;
using Ws2.EqualityComparison;

namespace Ws2.Async.Tests.Locks;

public class PooledLockTest
{
    private SemaphoreSlimPool semaphoreSlimPool = null!;
    private PooledLock pooledLock = null!;

    [SetUp]
    public void SetUp()
    {
        pooledLock = new PooledLock(semaphoreSlimPool = new SemaphoreSlimPool());
    }

    [TearDown]
    public void TearDown()
    {
        semaphoreSlimPool.Dispose();
    }

    [Test]
    [Timeout(1000)]
    public async Task TestPooledLockOnSameKeyWaits()
    {
        var key = Random.Shared.Next();
        var lockHolder =
            await pooledLock.AcquireAsync(key, EqualityComparers.Int32EqualityComparer, Timeout.InfiniteTimeSpan);

        var secondLockTask =
            pooledLock.AcquireAsync(key, EqualityComparers.Int32EqualityComparer, Timeout.InfiniteTimeSpan);

        secondLockTask.IsCompleted.Should().BeFalse();

        lockHolder.Dispose();

        var secondLockHolder = await secondLockTask;
        secondLockHolder.Dispose();
    }

    [Test]
    [Timeout(1000)]
    public async Task TestPooledLockOnDifferentKeys([Random(1)] int key1, [Random(1)] int key2)
    {
        using var lockHolder1 =
            await pooledLock.AcquireAsync(key1, EqualityComparers.Int32EqualityComparer, Timeout.InfiniteTimeSpan);
        using var lockHolder2 =
            await pooledLock.AcquireAsync(key2, EqualityComparers.Int32EqualityComparer, Timeout.InfiniteTimeSpan);

        Assert.Pass();
    }
}