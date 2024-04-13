using Microsoft.Extensions.DependencyInjection;

namespace Ws2.Hosting.Registrars;

public interface IStaticServiceRegistrar
{
    void Register(IServiceCollection serviceCollection);
}