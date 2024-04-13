using Medallion.Threading;
using Ws2.Async.Locks;

namespace Ws2.Async.DistributedLock;

public sealed class DistributedLockHolder : ILockHolder
{
    private readonly IDistributedSynchronizationHandle handle;

    public DistributedLockHolder(IDistributedSynchronizationHandle handle)
    {
        this.handle = handle;
    }

    public async Task ReleaseAsync(CancellationToken cancellationToken = default)
    {
        await DisposeAsync();
    }

    public void Dispose()
    {
        handle.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        await handle.DisposeAsync();
    }
}