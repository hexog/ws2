﻿using JetBrains.Annotations;

namespace Ws2.Async.Locks;

public interface ILockProvider : IDisposable, IAsyncDisposable
{
    [MustUseReturnValue]
    ValueTask<ILockHolder> AcquireAsync(ReadOnlyMemory<byte> key, TimeSpan timeout);

    [MustUseReturnValue]
    ValueTask<ILockHolder> AcquireAsync(ReadOnlyMemory<byte> key, CancellationToken cancellationToken);
}