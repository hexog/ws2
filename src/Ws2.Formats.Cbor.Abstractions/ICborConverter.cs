using System.Formats.Cbor;

namespace Ws2.Formats.Cbor;

// Generated models cache one converter instance per property; implementations must be thread-safe.
public interface ICborConverter<T>
{
    void Write(CborWriter writer, T value);

    T Read(CborReader reader);
}