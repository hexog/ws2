namespace Ws2.EqualityComparison.ByteMemory;

public class ByteArrayEqualityComparer : EqualityComparer<byte[]>
{
    public static readonly ByteArrayEqualityComparer Instance = new();

    public override bool Equals(byte[]? x, byte[]? y)
    {
        return ByteMemoryHelper.SequenceEquals(x, y);
    }

    public override int GetHashCode(byte[] obj)
    {
        return ByteMemoryHelper.GetLowCollisionHashCode(obj);
    }
}