namespace Ws2.Formats.Cbor;

internal enum CborValueKind
{
    Unsupported,
    Enum,
    Boolean,
    Byte,
    SByte,
    Int16,
    UInt16,
    Int32,
    UInt32,
    Int64,
    UInt64,
    Single,
    Double,
    String,
    Bytes,
    ByteArray,
    Collection,
    Nested
}