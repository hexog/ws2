namespace Ws2.Formats.Cbor;

[AttributeUsage(AttributeTargets.Property, Inherited = false)]
public sealed class CborKeyAttribute : Attribute
{
    public CborKeyAttribute(string name)
    {
        Name = name;
    }

    public CborKeyAttribute(int index)
    {
        Index = index;
    }

    public string? Name { get; }

    public int? Index { get; }
}