namespace Ws2.DependencyInjection.Abstractions;

public interface IDependencyRegistrar
{
    Type ServiceAttributeType { get; }
}

public interface IDependencyRegistrar<TServiceAttribute> : IDependencyRegistrar
    where TServiceAttribute : ServiceAttribute
{
    Type IDependencyRegistrar.ServiceAttributeType => typeof(TServiceAttribute);
}