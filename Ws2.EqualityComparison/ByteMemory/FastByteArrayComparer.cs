﻿namespace Ws2.EqualityComparison.ByteMemory;

public class FastByteArrayComparer(bool fromStart) : EqualityComparer<byte[]>
{
    public static readonly FastByteArrayComparer Instance = new(true);
    public static readonly FastByteArrayComparer InstanceReversed = new(false);

    public override bool Equals(byte[]? x, byte[]? y)
    {
        return ByteMemoryHelper.SequenceEquals(x, y);
    }

    public override int GetHashCode(byte[] obj)
    {
        if (fromStart)
        {
            return ByteMemoryHelper.GetStartHashCode(obj);
        }

        return ByteMemoryHelper.GetEndHashCode(obj);
    }
}