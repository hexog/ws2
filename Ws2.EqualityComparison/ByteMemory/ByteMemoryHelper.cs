using System.Diagnostics;
using System.IO.Hashing;

namespace Ws2.EqualityComparison.ByteMemory;

public static class ByteMemoryHelper
{
    public static int GetLowCollisionHashCode(ReadOnlySpan<byte> input)
    {
        return unchecked((int)XxHash3.HashToUInt64(input));
    }

    public static int GetStartHashCode(ReadOnlySpan<byte> input)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(input.Length);
        return input.Length switch
        {
            >= sizeof(int) => BitConverter.ToInt32(input),
            sizeof(short) => BitConverter.ToInt16(input),
            sizeof(byte) => input[0],
            3 => (input[0] << 16) | (input[1] << 8) | input[3],
            _ => throw new UnreachableException("Case validated by ArgumentOutOfRangeException.ThrowIfNegativeOrZero")
        };
    }

    public static int GetEndHashCode(ReadOnlySpan<byte> input)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(input.Length);
        return input.Length switch
        {
            var length and >= sizeof(int) => BitConverter.ToInt32(input[(length - sizeof(int))..]),
            sizeof(short) => BitConverter.ToInt16(input),
            sizeof(byte) => input[0],
            3 => (input[0] << 16) | (input[1] << 8) | input[3],
            _ => throw new UnreachableException("Case validated by ArgumentOutOfRangeException.ThrowIfNegativeOrZero")
        };
    }

    public static bool SequenceEquals(ReadOnlySpan<byte> left, ReadOnlySpan<byte> right)
    {
        return left.SequenceEqual(right);
    }

    public static bool SequenceEquals(byte[]? x, byte[]? y)
    {
        return ReferenceEquals(x, y) || (x is not null && y is not null && SequenceEquals(x, y));
    }
}