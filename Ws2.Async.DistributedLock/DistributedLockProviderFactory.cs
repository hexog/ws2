using Medallion.Threading;

namespace Ws2.Async.DistributedLock;

public class DistributedLockProviderFactory<TKey> : IDistributedLockFactory<TKey>
    where TKey : notnull
{
    private readonly IDistributedLockProvider distributedLockProvider;
    private readonly Converter<TKey, string> keyStringifier;

    public DistributedLockProviderFactory(IDistributedLockProvider distributedLockProvider, Converter<TKey, string> keyStringifier)
    {
        this.distributedLockProvider = distributedLockProvider;
        this.keyStringifier = keyStringifier;
    }

    public IDistributedLock Create(TKey key)
    {
        return distributedLockProvider.CreateLock(keyStringifier(key));
    }
}

public class DistributedLockProviderFactory : IDistributedLockFactory<string>
{
    private readonly IDistributedLockProvider distributedLockProvider;

    public DistributedLockProviderFactory(IDistributedLockProvider distributedLockProvider)
    {
        this.distributedLockProvider = distributedLockProvider;
    }

    public IDistributedLock Create(string key)
    {
        return distributedLockProvider.CreateLock(key);
    }
}