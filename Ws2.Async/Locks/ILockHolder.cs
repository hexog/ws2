namespace Ws2.Async.Locks;

public interface ILockHolder : IAsyncDisposable
{
    Task ReleaseAsync(CancellationToken cancellationToken = default);
}