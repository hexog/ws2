using FluentAssertions;
using Ws2.Async.Locks;
using Ws2.Async.Locks.PooledLocks;

namespace Ws2.Async.Tests.Locks;

[Timeout(1000)]
public class PooledSemaphoreLockKeyTest
{
    private PooledSemaphoreLockFactory pooledSemaphoreLockFactory = null!;

    [SetUp]
    public void SetUp()
    {
        pooledSemaphoreLockFactory = new PooledSemaphoreLockFactory(new SemaphoreSlimPool());
    }

    [TearDown]
    public async Task TearDown()
    {
        await pooledSemaphoreLockFactory.DisposeAsync();
    }

    [Test]
    public async Task TestPooledLockOnSameKeyWaits()
    {
        var key = Random.Shared.Next();
        var lockHolder =
            await pooledSemaphoreLockFactory.AcquireAsync(key, Timeout.InfiniteTimeSpan);

        var secondLockTask =
            pooledSemaphoreLockFactory.AcquireAsync(key, Timeout.InfiniteTimeSpan);

        secondLockTask.IsCompleted.Should().BeFalse();

        await lockHolder.ReleaseAsync();

        var secondLockHolder = await secondLockTask;
        await secondLockHolder.ReleaseAsync();

        Assert.Pass();
    }

    [Test]
    public async Task TestPooledLockOnDifferentKeys([Random(1)] int key1, [Random(1)] int key2)
    {
        await using var lockHolder1 =
            await pooledSemaphoreLockFactory.AcquireAsync(key1, Timeout.InfiniteTimeSpan);
        await using var lockHolder2 =
            await pooledSemaphoreLockFactory.AcquireAsync(key2, Timeout.InfiniteTimeSpan);

        Assert.Pass();
    }
}