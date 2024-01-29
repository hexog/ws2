using Ws2.Async.Locks;

namespace Ws2.Async.DistributedLock;

public class DistributedLock<TKey> : ILock<TKey>
    where TKey : notnull
{
    private readonly IDistributedLockFactory<TKey> distributedLockFactory;

    public DistributedLock(IDistributedLockFactory<TKey> distributedLockFactory)
    {
        this.distributedLockFactory = distributedLockFactory;
    }

    public async ValueTask<ILockHolder> AcquireAsync(TKey key, TimeSpan timeout)
    {
        var distributedSynchronizationHandle = await distributedLockFactory.Create(key).AcquireAsync(timeout);
        return new DistributedLockHolder(distributedSynchronizationHandle);
    }

    public async ValueTask<ILockHolder> AcquireAsync(TKey key, CancellationToken cancellationToken)
    {
        var distributedSynchronizationHandle =
            await distributedLockFactory.Create(key).AcquireAsync(cancellationToken: cancellationToken);
        return new DistributedLockHolder(distributedSynchronizationHandle);
    }

    public ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);
        return ValueTask.CompletedTask;
    }
}