using FluentAssertions;
using Medallion.Threading.FileSystem;
using Ws2.Async.DistributedLock;
using Ws2.Async.Locks;

namespace Ws2.Async.Tests.Locks;

[Timeout(1000)]
public class DistributedLockFactoryTest
{
#pragma warning disable NUnit1032
    private DistributedLockFactory lockFactoryFactory = null!;
#pragma warning enable NUnit1032


    private static readonly DirectoryInfo LockDirectory = new("./lock");

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        LockDirectory.Create();
    }

    [SetUp]
    public void SetUp()
    {
        lockFactoryFactory = new DistributedLockFactory(
            new FileDistributedSynchronizationProvider(LockDirectory)
        );
    }

    [TearDown]
    public void TearDown()
    {
        ((IDisposable)lockFactoryFactory).Dispose();
    }

    [Test]
    public async Task TestLockSameKey()
    {
        const int key = 12345;

        var lockHolderTask = lockFactoryFactory.AcquireAsync(key, Timeout.InfiniteTimeSpan);

        lockHolderTask.IsCompleted.Should().BeTrue();
        var lockHolder = await lockHolderTask;

        var secondLockHolder = lockFactoryFactory.AcquireAsync(key, Timeout.InfiniteTimeSpan);
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

        var lockHolderTask = lockFactoryFactory.AcquireAsync(key, Timeout.InfiniteTimeSpan);

        lockHolderTask.IsCompleted.Should().BeTrue();
        var lockHolder = await lockHolderTask;

        var secondLockHolder = lockFactoryFactory.AcquireAsync(key2, Timeout.InfiniteTimeSpan);
        secondLockHolder.IsCompleted.Should().BeTrue();

        await lockHolder.ReleaseAsync();
        await (await secondLockHolder).ReleaseAsync();

        Assert.Pass();
    }
}