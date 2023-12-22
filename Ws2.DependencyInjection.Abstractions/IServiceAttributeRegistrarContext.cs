using Microsoft.Extensions.DependencyInjection;

namespace Ws2.DependencyInjection.Abstractions;

public interface IServiceAttributeRegistrarContext
{
    IServiceCollection ServiceCollection { get; }

    IReadOnlyCollection<Type> Types { get; }

    IReadOnlyCollection<IServiceAttributeRegistrar> ServiceAttributeRegistrar { get; }

    IReadOnlyCollection<IServiceTypeImplementationRegistrar> ServiceTypeImplementationRegistrars { get; }

    Type? FIndType(string? typeName);

    IEnumerable<IServiceAttributeRegistrar> FindServiceAttributeRegistrars(Type serviceAttributeType);

    IEnumerable<IServiceTypeImplementationRegistrar> FindServiceImplementationRegistrars(Type serviceType);
}