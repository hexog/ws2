using System.IO.Hashing;
using Medallion.Threading;
using Ws2.Async.Locks;

namespace Ws2.Async.DistributedLock;

public sealed class DistributedLockFactory : ILockFactory
{
    private readonly IDistributedLockProvider distributedLockProvider;

    public DistributedLockFactory(IDistributedLockProvider distributedLockProvider)
    {
        this.distributedLockProvider = distributedLockProvider;
    }

    public async ValueTask<ILockHolder> AcquireAsync(ReadOnlyMemory<byte> key, TimeSpan timeout)
    {
        var distributedSynchronizationHandle =
            await distributedLockProvider.AcquireLockAsync(GetKeyString(key), timeout: timeout).ConfigureAwait(false);
        return new DistributedLockHolder(distributedSynchronizationHandle);
    }

    public async ValueTask<ILockHolder> AcquireAsync(ReadOnlyMemory<byte> key, CancellationToken cancellationToken)
    {
        var distributedSynchronizationHandle =
            await distributedLockProvider.AcquireLockAsync(GetKeyString(key), cancellationToken: cancellationToken)
                .ConfigureAwait(false);
        return new DistributedLockHolder(distributedSynchronizationHandle);
    }

    private static string GetKeyString(ReadOnlyMemory<byte> key)
    {
        return XxHash3.HashToUInt64(key.Span).ToString("x");
    }

    void IDisposable.Dispose()
    {
    }

    ValueTask IAsyncDisposable.DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }
}