namespace Ws2.DependencyInjection;

internal readonly record struct ValueServiceDescriptor(
    string ImplementationTypeName,
    object? ServiceKey,
    string? ServiceTypeName,
    ServiceLifetime Lifetime,
    bool? SharedSingletonInstance = null
)
{
    public readonly string ImplementationTypeName = ImplementationTypeName;
    public readonly object? ServiceKey = ServiceKey;
    public readonly string? ServiceTypeName = ServiceTypeName;
    public readonly ServiceLifetime Lifetime = Lifetime;
    public readonly bool? SharedSingletonInstance = SharedSingletonInstance;
}