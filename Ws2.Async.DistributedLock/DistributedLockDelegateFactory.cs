using Medallion.Threading;

namespace Ws2.Async.DistributedLock;

public class DistributedLockDelegateFactory<TKey> : IDistributedLockFactory<TKey>
    where TKey : notnull
{
    private readonly Func<TKey, IDistributedLock> factory;

    public DistributedLockDelegateFactory(Func<TKey, IDistributedLock> factory)
    {
        this.factory = factory;
    }

    public IDistributedLock Create(TKey key)
    {
        return factory(key);
    }
}