using FluentAssertions;
using Ws2.EqualityComparison.ByteMemory;

namespace Ws2.EqualityComparison.Tests.ByteMemory;

public class TestByteMemoryHelper
{
	[TestCase(new byte[] { 4 }, ExpectedResult = 851412594)]
	[TestCase(new byte[] { 123, 123 }, ExpectedResult = 976597369)]
	[TestCase(new byte[] { }, ExpectedResult = 953390274)]
	[TestCase(new byte[] { 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3 }, ExpectedResult = 1221939633)]
	public int TestGetLowCollisionHashCode(byte[] input)
	{
		return ByteMemoryHelper.GetLowCollisionHashCode(input);
	}

	[Test]
	public void TestGetLowCollisionHashCodeCollisions()
	{
		const int length = 20_000;
		var hashSet = new HashSet<int>(length);
		var random = new Random();

		for (var i = 0; i < length; i++)
		{
			var data = new byte[random.Next(512)];
			random.NextBytes(data);
			hashSet.Add(ByteMemoryHelper.GetLowCollisionHashCode(data));
		}

		if (hashSet.Count < length * 0.99)
		{
			Assert.Fail("Collision level higher than expected");
		}
	}

	[Repeat(10)]
	[Test]
	public void TestSequenceEquals()
	{
		var data = NextBytes();
		var data2 = CopyArray(data);

		var actual = ByteMemoryHelper.SequenceEquals(data, data2);
		actual.Should().BeTrue();
	}

	private static byte[] NextBytes(Random random, int length = 64)
	{
		var data = new byte[random.Next(512)];
		random.NextBytes(data);
		return data;
	}

	private static byte[] NextBytes(int length = 64)
	{
		return NextBytes(Random.Shared, length);
	}

	public static byte[] CopyArray(byte[] input)
	{
		var newArray = new byte[input.Length];
		Array.Copy(input, newArray, input.Length);
		return newArray;
	}
}