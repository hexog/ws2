namespace Ws2.DependencyInjection.Abstractions;

public interface IServiceRegistrar
{
    public void TryRegister(IServiceRegistrarContext context, Type type);
}