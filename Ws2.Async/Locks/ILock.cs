using JetBrains.Annotations;

namespace Ws2.Async.Locks;

public interface ILock : IAsyncDisposable
{
    [MustUseReturnValue]
    ValueTask<ILockHolder> AcquireAsync<TKey>(TKey key, IEqualityComparer<TKey> comparer, TimeSpan timeout)
        where TKey : notnull;

    [MustUseReturnValue]
    ValueTask<ILockHolder> AcquireAsync<TKey>(
        TKey key,
        IEqualityComparer<TKey> comparer,
        CancellationToken cancellationToken
    )
        where TKey : notnull;
}

public interface ILock<in TKey> : IAsyncDisposable
    where TKey : notnull
{
    [MustUseReturnValue]
    ValueTask<ILockHolder> AcquireAsync(TKey key, TimeSpan timeout);

    [MustUseReturnValue]
    ValueTask<ILockHolder> AcquireAsync(TKey key, CancellationToken cancellationToken);
}