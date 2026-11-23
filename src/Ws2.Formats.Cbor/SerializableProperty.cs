namespace Ws2.Formats.Cbor;

internal sealed class SerializableProperty : IEquatable<SerializableProperty>
{
    public SerializableProperty(
        string name, string typeName, CborValueKind kind, string? converterTypeName,
        string? cborMapKey = null, int? cborArrayIndex = null,
        CborCollectionKind? collectionKind = null, string? elementTypeName = null, CborValueKind? elementKind = null,
        bool isNullable = false, CborValueKind? enumUnderlyingKind = null, string? valueTypeName = null,
        bool isReferenceType = false, bool isNullableValueType = false, bool isElementNullable = false,
        bool isElementReferenceType = false, bool isElementNullableValueType = false,
        CborValueKind? elementEnumUnderlyingKind = null, string? elementValueTypeName = null
    )
    {
        Name = name;
        TypeName = typeName;
        Kind = kind;
        ConverterTypeName = converterTypeName;
        CborMapKey = cborMapKey;
        CborArrayIndex = cborArrayIndex;
        CollectionKind = collectionKind;
        ElementTypeName = elementTypeName;
        ElementKind = elementKind;
        IsNullable = isNullable;
        EnumUnderlyingKind = enumUnderlyingKind;
        ValueTypeName = valueTypeName ?? typeName;
        IsReferenceType = isReferenceType;
        IsNullableValueType = isNullableValueType;
        IsElementNullable = isElementNullable;
        IsElementReferenceType = isElementReferenceType;
        IsElementNullableValueType = isElementNullableValueType;
        ElementEnumUnderlyingKind = elementEnumUnderlyingKind;
        ElementValueTypeName = elementValueTypeName ?? elementTypeName;
    }

    public string Name { get; }

    public string TypeName { get; }

    public CborValueKind Kind { get; }

    public string? ConverterTypeName { get; }

    public string? CborMapKey { get; }

    public int? CborArrayIndex { get; }

    public CborCollectionKind? CollectionKind { get; }

    public string? ElementTypeName { get; }

    public CborValueKind? ElementKind { get; }

    public bool IsNullable { get; }

    public CborValueKind? EnumUnderlyingKind { get; }

    public string ValueTypeName { get; }

    public bool IsReferenceType { get; }

    public bool IsNullableValueType { get; }

    public bool IsElementNullable { get; }

    public bool IsElementReferenceType { get; }

    public bool IsElementNullableValueType { get; }

    public CborValueKind? ElementEnumUnderlyingKind { get; }

    public string? ElementValueTypeName { get; }

    public bool Equals(SerializableProperty? other)
    {
        return other is not null &&
            Name == other.Name && TypeName == other.TypeName && Kind == other.Kind &&
            ConverterTypeName == other.ConverterTypeName &&
            CborMapKey == other.CborMapKey && CborArrayIndex == other.CborArrayIndex &&
            CollectionKind == other.CollectionKind && ElementTypeName == other.ElementTypeName && ElementKind == other.ElementKind &&
            IsNullable == other.IsNullable && EnumUnderlyingKind == other.EnumUnderlyingKind &&
            ValueTypeName == other.ValueTypeName && IsReferenceType == other.IsReferenceType &&
            IsNullableValueType == other.IsNullableValueType && IsElementNullable == other.IsElementNullable &&
            IsElementReferenceType == other.IsElementReferenceType && IsElementNullableValueType == other.IsElementNullableValueType &&
            ElementEnumUnderlyingKind == other.ElementEnumUnderlyingKind && ElementValueTypeName == other.ElementValueTypeName;
    }

    public override bool Equals(object? obj)
    {
        return obj is SerializableProperty other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Name, TypeName, Kind, ConverterTypeName,
            HashCode.Combine(CborMapKey, CborArrayIndex),
            HashCode.Combine(CollectionKind, ElementTypeName, ElementKind, IsNullable, EnumUnderlyingKind,
                HashCode.Combine(ValueTypeName, IsReferenceType, IsNullableValueType, IsElementNullable,
                    IsElementReferenceType, IsElementNullableValueType, ElementEnumUnderlyingKind, ElementValueTypeName)));
    }
}