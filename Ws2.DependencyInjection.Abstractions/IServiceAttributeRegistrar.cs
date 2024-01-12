namespace Ws2.DependencyInjection.Abstractions;

public interface IServiceAttributeRegistrar
{
    Type ServiceAttributeType { get; }

    void Register(IServiceAttributeRegistrarContext context, Type type, object serviceAttribute);
}

public interface IServiceAttributeRegistrar<in TServiceAttribute> : IServiceAttributeRegistrar
    where TServiceAttribute : Attribute
{
    Type IServiceAttributeRegistrar.ServiceAttributeType => typeof(TServiceAttribute);

    void IServiceAttributeRegistrar.Register(
        IServiceAttributeRegistrarContext context,
        Type type,
        object serviceAttribute
    )
    {
        Register(context, type, (TServiceAttribute)serviceAttribute);
    }

    void Register(IServiceAttributeRegistrarContext context, Type type, TServiceAttribute serviceAttribute);
}