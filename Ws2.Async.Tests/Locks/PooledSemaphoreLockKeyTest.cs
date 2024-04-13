using FluentAssertions;
using Ws2.Async.Locks.PooledLocks;

namespace Ws2.Async.Tests.Locks;

[Timeout(1000)]
public class PooledSemaphoreLockKeyTest
{
    private PooledSemaphoreLock<int> pooledSemaphoreLock = null!;

    [SetUp]
    public void SetUp()
    {
        pooledSemaphoreLock = new PooledSemaphoreLock<int>(new SemaphoreSlimPool(), EqualityComparer<int>.Default);
    }

    [TearDown]
    public async Task TearDown()
    {
        await pooledSemaphoreLock.DisposeAsync();
    }

    [Test]
    public async Task TestPooledLockOnSameKeyWaits()
    {
        var key = Random.Shared.Next();
        var lockHolder =
            await pooledSemaphoreLock.AcquireAsync(key, Timeout.InfiniteTimeSpan);

        var secondLockTask =
            pooledSemaphoreLock.AcquireAsync(key, Timeout.InfiniteTimeSpan);

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
            await pooledSemaphoreLock.AcquireAsync(key1, Timeout.InfiniteTimeSpan);
        await using var lockHolder2 =
            await pooledSemaphoreLock.AcquireAsync(key2, Timeout.InfiniteTimeSpan);

        Assert.Pass();
    }
}