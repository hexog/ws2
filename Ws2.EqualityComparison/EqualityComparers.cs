using Ws2.EqualityComparison.ByteMemory;

namespace Ws2.EqualityComparison;

public static class EqualityComparers
{
    public static ByteArrayEqualityComparer ByteArrayEqualityComparer => ByteArrayEqualityComparer.Instance;

    public static FastByteArrayComparer ByteArrayFastEqualityComparer => FastByteArrayComparer.Instance;

    public static FastByteArrayComparer ByteArrayFastReversedEqualityComparer => FastByteArrayComparer.InstanceReversed;

    public static IntEqualityComparer Int32EqualityComparer => IntEqualityComparer.Instance;
}