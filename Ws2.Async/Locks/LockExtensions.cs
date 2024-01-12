using Ws2.EqualityComparison;

namespace Ws2.Async.Locks;

public static class LockExtensions
{
    public static ValueTask<ILockHolder> AcquireAsync(
        this ILock @lock,
        byte[] bytes,
        CancellationToken cancellationToken
    )
    {
        return @lock.AcquireAsync(bytes, EqualityComparers.ByteArrayEqualityComparer, cancellationToken);
    }

    public static ValueTask<ILockHolder> AcquireAsync(
        this ILock @lock,
        int key,
        CancellationToken cancellationToken
    )
    {
        return @lock.AcquireAsync(key, EqualityComparers.Int32EqualityComparer, cancellationToken);
    }

    public static ValueTask<ILockHolder> AcquireAsync(
        this ILock @lock,
        string key,
        CancellationToken cancellationToken
    )
    {
        return @lock.AcquireAsync(key, StringComparer.Ordinal, cancellationToken);
    }
}