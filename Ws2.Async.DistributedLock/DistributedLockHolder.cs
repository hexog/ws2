using Medallion.Threading;
using Ws2.Async.Locks;

namespace Ws2.Async.DistributedLock;

public class DistributedLockHolder : ILockHolder
{
    private readonly IDistributedSynchronizationHandle handle;
    private int isDisposed;

    public DistributedLockHolder(IDistributedSynchronizationHandle handle)
    {
        this.handle = handle;
    }

    public async ValueTask DisposeAsync()
    {
        await ReleaseAsync();
        GC.SuppressFinalize(this);
    }

    public async Task ReleaseAsync(CancellationToken cancellationToken = default)
    {
        if (Interlocked.Exchange(ref isDisposed, 1) == 0)
        {
            await handle.DisposeAsync();
        }
    }
}