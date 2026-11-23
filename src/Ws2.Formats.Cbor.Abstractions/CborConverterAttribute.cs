namespace Ws2.Formats.Cbor;

[AttributeUsage(AttributeTargets.Property, Inherited = false)]
public sealed class CborConverterAttribute(Type converterType) : Attribute
{
    public Type ConverterType { get; } = converterType;
}