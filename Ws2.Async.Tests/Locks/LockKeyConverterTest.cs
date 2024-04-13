using System.Text;
using System.Text.Json;
using FluentAssertions;
using Ws2.Async.Locks;
using Ws2.Async.Locks.PooledLocks;

namespace Ws2.Async.Tests.Locks;

public class LockKeyConverterTest
{
    public class ComplexKey
    {
        public int A { get; set; }
        public string? B { get; set; }
    }

    [Test]
    public void TestAddNewKeyConverterExecutedOnAcquire()
    {
        var added = LockKeyConverterDictionary.TryAddConverter<ComplexKey>(
            x =>
            {
                var buffer = new MemoryStream();
                JsonSerializer.Serialize(buffer, x);
                return buffer.GetBuffer();
            }
        );

        added.Should().BeTrue();

        using var lockFactory = new PooledSemaphoreLockFactory(new SemaphoreSlimPool());

        var result = lockFactory.AcquireAsync(new ComplexKey(), Timeout.InfiniteTimeSpan);
        result.IsCompleted.Should().BeTrue();

        var result2 = lockFactory.AcquireAsync(new ComplexKey(), Timeout.InfiniteTimeSpan);
        result2.IsCompleted.Should().BeFalse();

        var result3 = lockFactory.AcquireAsync(new ComplexKey { A = 1 }, Timeout.InfiniteTimeSpan);
        result3.IsCompleted.Should().BeTrue();
    }
}