namespace Ws2.DependencyInjection;

internal readonly record struct RegisteredSingleton(
    string ImplementationTypeName,
    object? Key
)
{
    public readonly string ImplementationTypeName = ImplementationTypeName;
    public readonly object? Key = Key;
}