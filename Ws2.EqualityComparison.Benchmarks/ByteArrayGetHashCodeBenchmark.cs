using BenchmarkDotNet.Attributes;
using Ws2.EqualityComparison.ByteMemory;

namespace Ws2.EqualityComparison.Benchmarks;

[SimpleJob]
public class ByteArrayGetHashCodeBenchmark
{
	[Params(1, 32, 128, 1024)]
	public int DataLength { get; set; }

	private byte[] data = null!;

	[IterationSetup]
	public void Setup()
	{
		data = new byte[DataLength];
		Random.Shared.NextBytes(data);
	}

	[Benchmark(Baseline = true)]
	public int StandardArrayGetHashCodeBenchmark()
	{
		var hashCode = 0;
		for (var i = 0; i < 5000; i++)
		{
			unchecked
			{
				hashCode += ByteMemoryHelper.GetLowCollisionHashCode(data);
			}
		}

		return hashCode;
	}

	[Benchmark]
	public int FastArrayGetHashCodeBenchmark()
	{
		var hashCode = 0;
		for (var i = 0; i < 5000; i++)
		{
			unchecked
			{
				hashCode += ByteMemoryHelper.GetStartHashCode(data);
			}
		}

		return hashCode;
	}

	[Benchmark]
	public int FastReversedArrayGetHashCodeBenchmark()
	{
		var hashCode = 0;
		for (var i = 0; i < 5000; i++)
		{
			unchecked
			{
				hashCode += ByteMemoryHelper.GetEndHashCode(data);
			}
		}

		return hashCode;
	}
}