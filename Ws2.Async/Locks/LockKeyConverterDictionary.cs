using System.Collections.Frozen;
using System.Diagnostics;
using System.Text;

namespace Ws2.Async.Locks;

public class LockKeyConverterDictionary : ILockKeyConverterDictionary
{
    private IDictionary<Type, Delegate> converters = new Dictionary<Type, Delegate>
    {
        [typeof(int)] = (Converter<int, ReadOnlyMemory<byte>>)(static x => BitConverter.GetBytes(x)),
        [typeof(uint)] = (Converter<uint, ReadOnlyMemory<byte>>)(static x => BitConverter.GetBytes(x)),
        [typeof(long)] = (Converter<long, ReadOnlyMemory<byte>>)(static x => BitConverter.GetBytes(x)),
        [typeof(ulong)] = (Converter<ulong, ReadOnlyMemory<byte>>)(static x => BitConverter.GetBytes(x)),
        [typeof(short)] = (Converter<short, ReadOnlyMemory<byte>>)(static x => BitConverter.GetBytes(x)),
        [typeof(ushort)] = (Converter<ushort, ReadOnlyMemory<byte>>)(static x => BitConverter.GetBytes(x)),
        [typeof(byte)] = (Converter<byte, ReadOnlyMemory<byte>>)(static x => new[] { x }),
        [typeof(sbyte)] = (Converter<sbyte, ReadOnlyMemory<byte>>)(static x => new[] { unchecked((byte)x) }),
        [typeof(float)] = (Converter<float, ReadOnlyMemory<byte>>)(static x => BitConverter.GetBytes(x)),
        [typeof(double)] = (Converter<double, ReadOnlyMemory<byte>>)(static x => BitConverter.GetBytes(x)),

        [typeof(string)] = (Converter<string, ReadOnlyMemory<byte>>)(static x => Encoding.Unicode.GetBytes(x)),
        [typeof(byte[])] = (Converter<byte[], ReadOnlyMemory<byte>>)(static x => x),
        [typeof(Memory<byte>)] = (Converter<Memory<byte>, ReadOnlyMemory<byte>>)(static x => x),
        [typeof(ReadOnlyMemory<byte>)] = (Converter<ReadOnlyMemory<byte>, ReadOnlyMemory<byte>>)(static x => x)
    };

    public static LockKeyConverterDictionary Instance { get; set; } = new();

    public IDictionary<Type, Delegate> Converters => converters;

    public void MakeReadOnly()
    {
        converters = converters.ToFrozenDictionary();
    }

    public static bool TryAddConverter<TKey>(Converter<TKey, ReadOnlyMemory<byte>> converter)
    {
        return Instance.Converters.TryAdd(typeof(TKey), converter);
    }

    public static ReadOnlyMemory<byte> Convert<TKey>(TKey value)
    {
        if (Instance.Converters.TryGetValue(typeof(TKey), out var converter))
        {
            var typedConverter = converter as Converter<TKey, ReadOnlyMemory<byte>>;
            Debug.Assert(typedConverter is not null);
            return typedConverter(value);
        }

        throw new NotSupportedException($"Converter for type {typeof(TKey).Name} is not registered");
    }

}