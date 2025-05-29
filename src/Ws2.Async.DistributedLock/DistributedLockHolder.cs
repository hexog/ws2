﻿using Medallion.Threading;
using Ws2.Async.Locks;

namespace Ws2.Async.DistributedLock;

public sealed class DistributedLockHolder(IDistributedSynchronizationHandle handle) : ILockHolder
{
    public async Task ReleaseAsync(CancellationToken cancellationToken = default)
    {
        await DisposeAsync().ConfigureAwait(false);
    }

    public void Dispose()
    {
        handle.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        await handle.DisposeAsync().ConfigureAwait(false);
    }
}