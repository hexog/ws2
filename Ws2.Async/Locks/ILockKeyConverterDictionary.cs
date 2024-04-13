namespace Ws2.Async.Locks;

public interface ILockKeyConverterDictionary
{
    IDictionary<Type, Delegate> Converters { get; }
}