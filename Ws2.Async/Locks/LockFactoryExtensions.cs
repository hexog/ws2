namespace Ws2.Async.Locks;

public static class LockFactoryExtensions
{
    public static async ValueTask<ILockHolder> AcquireAsync<TKey>(
        this ILockFactory lockFactory,
        TKey key,
        CancellationToken cancellationToken
    )
    {
        return await lockFactory.AcquireAsync(LockKeyConverterDictionary.Convert(key), cancellationToken);
    }

    public static async ValueTask<ILockHolder> AcquireAsync<TKey>(
        this ILockFactory lockFactory,
        TKey key,
        TimeSpan timeout
    )
    {
        return await lockFactory.AcquireAsync(LockKeyConverterDictionary.Convert(key), timeout);
    }
}