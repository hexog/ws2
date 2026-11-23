using Ws2.Formats.Cbor;

namespace PackageConsumer.Models;

[CborArray]
public partial class Child
{
    [CborKey(0)]
    public int Value { get; }

    public Child(int value) => Value = value;
}

[CborMap]
public partial class Message
{
    [CborKey("id")]
    public int Id { get; }

    [CborKey("name")]
    public string? Name { get; }

    [CborKey("values")]
    public IReadOnlyList<int> Values { get; }

    [CborKey("child")]
    public Child? Child { get; }

    [CborKey("version"), CborConverter(typeof(VersionConverter))]
    public Version Version { get; }

    public Message(int id, string? name, IReadOnlyList<int> values, Child? child, Version version)
    {
        Id = id;
        Name = name;
        Values = values;
        Child = child;
        Version = version;
    }
}

[CborMap]
public partial class MutableMessage
{
    [CborKey("number")]
    public int Number { get; set; }

    [CborKey("label")]
    public string? Label { get; init; }

    [CborKey("values")]
    public IReadOnlyList<int> Values { get; set; } = Array.Empty<int>();

    [CborKey("version"), CborConverter(typeof(VersionConverter))]
    public Version Version { get; init; } = new(0, 0);

    public MutableMessage() { }
}
