using JetBrains.Annotations;

namespace Ws2.Async.Locks;

public interface ILock
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