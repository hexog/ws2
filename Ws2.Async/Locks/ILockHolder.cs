namespace Ws2.Async.Locks;

public interface ILockHolder : IDisposable, IAsyncDisposable
{
    Task ReleaseAsync(CancellationToken cancellationToken = default);
}