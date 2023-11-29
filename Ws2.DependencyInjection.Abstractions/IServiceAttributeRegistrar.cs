namespace Ws2.DependencyInjection.Abstractions;

public interface IServiceAttributeRegistrar
{
    Type ServiceAttributeType { get; }

    void Register(ServiceAttributeRegistrarContext context, Type type, object serviceAttribute);
}

public interface IServiceAttributeRegistrar<in TServiceAttribute> : IServiceAttributeRegistrar
    where TServiceAttribute : ServiceAttribute
{
    Type IServiceAttributeRegistrar.ServiceAttributeType => typeof(TServiceAttribute);

    void IServiceAttributeRegistrar.Register(
        ServiceAttributeRegistrarContext context,
        Type type,
        object serviceAttribute
    )
    {
        Register(context, type, (TServiceAttribute)serviceAttribute);
    }

    void Register(ServiceAttributeRegistrarContext context, Type type, TServiceAttribute serviceAttribute);
}