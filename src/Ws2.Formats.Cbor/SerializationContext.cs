namespace Ws2.Formats.Cbor;

internal sealed class SerializationContext : IEquatable<SerializationContext>
{
    public SerializationContext(
        string @namespace, string className, string declarationKind,
        CborSerializationFormat format,
        EquatableList<SerializableProperty> properties, EquatableList<int>? constructorPropertyIndexes = null,
        EquatableList<int>? mapPropertyIndexes = null, string? hintName = null, string? generatedPrefix = null,
        bool usePropertyInitializer = false
    )
    {
        Namespace = @namespace;
        ClassName = className;
        DeclarationKind = declarationKind;
        Format = format;
        Properties = properties;
        ConstructorPropertyIndexes = constructorPropertyIndexes ?? new EquatableList<int>([]);
        MapPropertyIndexes = mapPropertyIndexes ?? new EquatableList<int>([]);
        HintName = hintName ?? className + ".Cbor.g.cs";
        GeneratedPrefix = generatedPrefix ?? "__CborGenerated_";
        UsePropertyInitializer = usePropertyInitializer;
    }

    public string Namespace { get; }

    public string ClassName { get; }

    public string DeclarationKind { get; }

    public CborSerializationFormat Format { get; }

    public EquatableList<SerializableProperty> Properties { get; }

    public EquatableList<int> ConstructorPropertyIndexes { get; }

    public EquatableList<int> MapPropertyIndexes { get; }

    public string HintName { get; }

    public string GeneratedPrefix { get; }

    public bool UsePropertyInitializer { get; }

    public bool Equals(SerializationContext? other)
    {
        return other is not null &&
            Namespace == other.Namespace && ClassName == other.ClassName &&
            DeclarationKind == other.DeclarationKind && Format == other.Format &&
            Properties.Equals(other.Properties) && ConstructorPropertyIndexes.Equals(other.ConstructorPropertyIndexes) &&
            MapPropertyIndexes.Equals(other.MapPropertyIndexes) && HintName == other.HintName &&
            GeneratedPrefix == other.GeneratedPrefix && UsePropertyInitializer == other.UsePropertyInitializer;
    }

    public override bool Equals(object? obj)
    {
        return obj is SerializationContext other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Namespace, ClassName, DeclarationKind, Format, Properties, ConstructorPropertyIndexes,
            MapPropertyIndexes, HashCode.Combine(HintName, GeneratedPrefix, UsePropertyInitializer));
    }
}