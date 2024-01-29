using Medallion.Threading;

namespace Ws2.Async.DistributedLock;

public interface IDistributedLockFactory<in TKey>
    where TKey : notnull
{
    IDistributedLock Create(TKey key);
}