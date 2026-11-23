using System.Collections;

namespace Ws2.Formats.Cbor;

/// <summary>A copied, immutable sequence with structural equality.</summary>
public sealed class EquatableList<T> : IReadOnlyList<T>, IEquatable<EquatableList<T>>
{
    private readonly T[] items;

    public EquatableList(IEnumerable<T> collection)
    {
        items = [.. collection];
    }

    public bool Equals(EquatableList<T>? other)
    {
        if (other is null || Count != other.Count)
        {
            return false;
        }

        for (var i = 0; i < Count; i++)
        {
            if (!EqualityComparer<T>.Default.Equals(items[i], other.items[i]))
            {
                return false;
            }
        }

        return true;
    }

    public int Count => items.Length;

    public T this[int index] => items[index];

    public IEnumerator<T> GetEnumerator()
    {
        return ((IEnumerable<T>)items).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return items.GetEnumerator();
    }

    public override bool Equals(object? obj)
    {
        return obj is EquatableList<T> other && Equals(other);
    }

    public override int GetHashCode()
    {
        HashCode hashCode = new();
        hashCode.Add(Count);
        foreach (var item in items)
        {
            hashCode.Add(item);
        }

        return hashCode.ToHashCode();
    }
}