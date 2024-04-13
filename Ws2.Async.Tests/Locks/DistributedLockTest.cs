using FluentAssertions;
using Medallion.Threading.FileSystem;
using Ws2.Async.DistributedLock;

namespace Ws2.Async.Tests.Locks;

[Timeout(1000)]
public class DistributedLockTest
{
    private DistributedLock<int> lockFactory = null!;

    private static readonly DirectoryInfo LockDirectory = new("./lock");

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        LockDirectory.Create();
    }

    [SetUp]
    public void SetUp()
    {
        lockFactory = new DistributedLock<int>(
            new DistributedLockProviderFactory<int>(
                new FileDistributedSynchronizationProvider(LockDirectory),
                static key => key.ToString()
            )
        );
    }

    [TearDown]
    public async Task TearDown()
    {
        await lockFactory.DisposeAsync();
    }

    [Test]
    public async Task TestLockSameKey()
    {
        const int key = 12345;

        var lockHolderTask = lockFactory.AcquireAsync(key, Timeout.InfiniteTimeSpan);

        lockHolderTask.IsCompleted.Should().BeTrue();
        var lockHolder = await lockHolderTask;

        var secondLockHolder = lockFactory.AcquireAsync(key, Timeout.InfiniteTimeSpan);
        secondLockHolder.IsCompleted.Should().BeFalse();

        await lockHolder.ReleaseAsync();

        await using (await secondLockHolder)
        {
            Assert.Pass();
        }
    }

    [Test]
    public async Task TestLockDifferentKeys()
    {
        const int key = 12345;
        const int key2 = 123456;

        var lockHolderTask = lockFactory.AcquireAsync(key, Timeout.InfiniteTimeSpan);

        lockHolderTask.IsCompleted.Should().BeTrue();
        var lockHolder = await lockHolderTask;

        var secondLockHolder = lockFactory.AcquireAsync(key2, Timeout.InfiniteTimeSpan);
        secondLockHolder.IsCompleted.Should().BeTrue();

        await lockHolder.ReleaseAsync();
        await (await secondLockHolder).ReleaseAsync();

        Assert.Pass();
    }
}