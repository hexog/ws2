using Medallion.Threading.FileSystem;
using Ws2.Async.DistributedLock;
using Ws2.Async.Locks;

namespace Ws2.Async.Tests.Locks;

[CancelAfter(1000)]
public class DistributedLockProviderTest
{
#pragma warning disable NUnit1032
    private DistributedLockProvider lockProviderProvider = null!;
#pragma warning restore NUnit1032


    private static readonly DirectoryInfo LockDirectory = new("./lock");

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        LockDirectory.Create();
    }

    [SetUp]
    public void SetUp()
    {
        lockProviderProvider = new DistributedLockProvider(
            new FileDistributedSynchronizationProvider(LockDirectory)
        );
    }

    [TearDown]
    public void TearDown()
    {
        ((IDisposable)lockProviderProvider).Dispose();
    }

    [Test]
    public async Task TestLockSameKey()
    {
        const int key = 12345;

        var lockHolderTask = lockProviderProvider.AcquireAsync(key, Timeout.InfiniteTimeSpan);

        Assert.That(lockHolderTask.IsCompleted, Is.True);
        var lockHolder = await lockHolderTask;

        var secondLockHolder = lockProviderProvider.AcquireAsync(key, Timeout.InfiniteTimeSpan);
        Assert.That(secondLockHolder.IsCompleted, Is.False);

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

        var lockHolderTask = lockProviderProvider.AcquireAsync(key, Timeout.InfiniteTimeSpan);

        Assert.That(lockHolderTask.IsCompleted, Is.True);
        var lockHolder = await lockHolderTask;

        var secondLockHolder = lockProviderProvider.AcquireAsync(key2, Timeout.InfiniteTimeSpan);
        Assert.That(secondLockHolder.IsCompleted, Is.True);

        await lockHolder.ReleaseAsync();
        await (await secondLockHolder).ReleaseAsync();

        Assert.Pass();
    }
}