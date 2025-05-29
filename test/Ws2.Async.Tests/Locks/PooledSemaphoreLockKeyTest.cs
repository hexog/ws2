using Ws2.Async.Locks;
using Ws2.Async.Locks.PooledLocks;

namespace Ws2.Async.Tests.Locks;

[CancelAfter(1000)]
public class PooledSemaphoreLockKeyTest
{
    private PooledSemaphoreLockProvider pooledSemaphoreLockProvider = null!;

    [SetUp]
    public void SetUp()
    {
        pooledSemaphoreLockProvider = new PooledSemaphoreLockProvider(new SemaphoreSlimPool());
    }

    [TearDown]
    public async Task TearDown()
    {
        await pooledSemaphoreLockProvider.DisposeAsync();
    }

    [Test]
    public async Task TestPooledLockOnSameKeyWaits()
    {
        var key = Random.Shared.Next();
        var lockHolder =
            await pooledSemaphoreLockProvider.AcquireAsync(key, Timeout.InfiniteTimeSpan);

        var secondLockTask =
            pooledSemaphoreLockProvider.AcquireAsync(key, Timeout.InfiniteTimeSpan);

        Assert.That(secondLockTask.IsCompleted, Is.False);

        await lockHolder.ReleaseAsync();

        var secondLockHolder = await secondLockTask;
        await secondLockHolder.ReleaseAsync();

        Assert.Pass();
    }

    [Test]
    public async Task TestPooledLockOnDifferentKeys([Random(1)] int key1, [Random(1)] int key2)
    {
        await using var lockHolder1 =
            await pooledSemaphoreLockProvider.AcquireAsync(key1, Timeout.InfiniteTimeSpan);
        await using var lockHolder2 =
            await pooledSemaphoreLockProvider.AcquireAsync(key2, Timeout.InfiniteTimeSpan);

        Assert.Pass();
    }
}