using System.Formats.Cbor;
using Ws2.Formats.Cbor;

public sealed class VersionConverter : ICborConverter<Version>
{
    public void Write(CborWriter writer, Version value) => writer.WriteTextString(value.ToString());

    public Version Read(CborReader reader) => Version.Parse(reader.ReadTextString());
}
