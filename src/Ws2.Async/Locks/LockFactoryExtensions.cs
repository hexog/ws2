using System.Text;

namespace Ws2.Async.Locks;

public static class LockFactoryExtensions
{
    public static async ValueTask<ILockHolder> AcquireAsync(
        this ILockProvider lockProvider,
        Guid key,
        CancellationToken cancellationToken
    )
    {
        return await lockProvider.AcquireAsync(key.ToByteArray(), cancellationToken).ConfigureAwait(false);
    }

    public static async ValueTask<ILockHolder> AcquireAsync(
        this ILockProvider lockProvider,
        int key,
        TimeSpan timeout
    )
    {
        return await lockProvider.AcquireAsync(BitConverter.GetBytes(key), timeout).ConfigureAwait(false);
    }

    public static async ValueTask<ILockHolder> AcquireAsync(
        this ILockProvider lockProvider,
        uint key,
        TimeSpan timeout
    )
    {
        return await lockProvider.AcquireAsync(BitConverter.GetBytes(key), timeout).ConfigureAwait(false);
    }
    public static async ValueTask<ILockHolder> AcquireAsync(
        this ILockProvider lockProvider,
        long key,
        TimeSpan timeout
    )
    {
        return await lockProvider.AcquireAsync(BitConverter.GetBytes(key), timeout).ConfigureAwait(false);
    }

    public static async ValueTask<ILockHolder> AcquireAsync(
        this ILockProvider lockProvider,
        ulong key,
        TimeSpan timeout
    )
    {
        return await lockProvider.AcquireAsync(BitConverter.GetBytes(key), timeout).ConfigureAwait(false);
    }

    public static async ValueTask<ILockHolder> AcquireAsync(
        this ILockProvider lockProvider,
        string key,
        TimeSpan timeout
    )
    {
        return await lockProvider.AcquireAsync(Encoding.Unicode.GetBytes(key), timeout).ConfigureAwait(false);
    }
}