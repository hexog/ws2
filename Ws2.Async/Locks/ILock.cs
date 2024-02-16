using JetBrains.Annotations;

namespace Ws2.Async.Locks;

public interface ILock<in TKey> : IAsyncDisposable
    where TKey : notnull
{
    [MustUseReturnValue]
    ValueTask<ILockHolder> AcquireAsync(TKey key, TimeSpan timeout);

    [MustUseReturnValue]
    ValueTask<ILockHolder> AcquireAsync(TKey key, CancellationToken cancellationToken);
}