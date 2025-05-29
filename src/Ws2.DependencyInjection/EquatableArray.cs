namespace Ws2.DependencyInjection;

internal readonly struct EquatableArray<T>(T[]? array)
    : IEquatable<EquatableArray<T>>
    where T : IEquatable<T>
{
    public readonly T[]? Array = array;

    public static EquatableArray<T> Empty => new([]);

    public bool IsEmpty => Array.AsSpan().IsEmpty;

    public bool Equals(EquatableArray<T> other)
    {
        return Array.AsSpan().SequenceEqual(other.Array.AsSpan());
    }

    public override bool Equals(object? obj)
    {
        return obj is EquatableArray<T> other && Equals(other);
    }

    public override int GetHashCode()
    {
        var array = Array;
        if (array is null)
        {
            return 0;
        }

        var hashCode = new HashCode();
        foreach (var value in array)
        {
            hashCode.Add(value);
        }

        return hashCode.ToHashCode();
    }
}