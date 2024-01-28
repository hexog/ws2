using Microsoft.Extensions.DependencyInjection;

namespace Ws2.DependencyInjection.Abstractions;

public interface IServiceRegistrarContext
{
    IServiceCollection ServiceCollection { get; }

    IReadOnlyCollection<Type> Types { get; }

    IReadOnlyCollection<IServiceRegistrar> ServiceRegistrars { get; }

    Type? FindType(string? typeName);

    bool IsValidImplementationType(Type implementationType);
}