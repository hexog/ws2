namespace Ws2.EqualityComparison.ByteMemory;

public class ReadOnlyByteMemoryEqualityComparer : EqualityComparer<ReadOnlyMemory<byte>>
{
    public static readonly ReadOnlyByteMemoryEqualityComparer Instance = new();

    public override bool Equals(ReadOnlyMemory<byte> x, ReadOnlyMemory<byte> y)
    {
        return ByteMemoryHelper.SequenceEquals(x.Span, y.Span);
    }

    public override int GetHashCode(ReadOnlyMemory<byte> obj)
    {
        return ByteMemoryHelper.GetLowCollisionHashCode(obj.Span);
    }
}