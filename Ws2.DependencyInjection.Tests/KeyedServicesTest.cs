using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Ws2.DependencyInjection.Abstractions;
using Ws2.DependencyInjection.LifetimeAttributes;

namespace Ws2.DependencyInjection.Tests;

public class KeyedServicesTest
{
    public interface IInterface1
    {
    }

    [ScopedService(ServiceKey = "foo")]
    [ScopedService(ServiceKey = "bar")]
    [ScopedService<IInterface1>(ServiceKey = "barC")]
    public class ScopedService1 : IInterface1
    {
    }

    [TransientService(ServiceKey = "foo")]
    [TransientService(ServiceKey = "bar")]
    [TransientService<IInterface1>(ServiceKey = "barT")]
    [TransientService<IInterface1>(ServiceKey = "bazT")]
    public class TransientService1 : IInterface1
    {
    }

    [SingletonService(ServiceKey = "foo")]
    [SingletonService(ServiceKey = "bar")]
    [SingletonService(ServiceKey = "baz", InstanceSharing = SingletonServiceInstanceSharing.KeyedInstance)]
    [SingletonService<IInterface1>(ServiceKey = "barS")]
    [SingletonService<IInterface1>(ServiceKey = "bazS")]
    public class SingletonService1 : IInterface1
    {
    }

    [Test]
    public void TestKeyedScopedService()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddServicesFromTypes(
            new[] { typeof(IInterface1), typeof(ScopedService1), typeof(TransientService1), typeof(SingletonService1) },
            ServiceRegistrationServiceCollectionExtensions.DefaultAttributeRegistrars
        );

        var serviceProvider = serviceCollection.BuildServiceProvider();
        var serviceScope = serviceProvider.CreateScope();
        var services = serviceScope.ServiceProvider;

        var scopedFoo = services.GetRequiredKeyedService<ScopedService1>("foo");
        var scopedFoo2 = services.GetRequiredKeyedService<ScopedService1>("foo");
        var scopedBar = services.GetRequiredKeyedService<ScopedService1>("bar");
        var scopedInterface = services.GetRequiredKeyedService<IInterface1>("barC");
        var scopedInterface2 = services.GetRequiredKeyedService<IInterface1>("barC");

        ReferenceEquals(scopedFoo, scopedFoo2).Should().BeTrue();
        ReferenceEquals(scopedFoo, scopedBar).Should().BeFalse();
        ReferenceEquals(scopedFoo, scopedInterface).Should().BeFalse();
        ReferenceEquals(scopedInterface, scopedInterface2).Should().BeTrue();

        var transientFoo = services.GetRequiredKeyedService<TransientService1>("foo");
        var transientFoo2 = services.GetRequiredKeyedService<TransientService1>("foo");
        var transientBar = services.GetRequiredKeyedService<TransientService1>("bar");
        var transientInterface = services.GetRequiredKeyedService<IInterface1>("barT");
        var transientInterface2 = services.GetRequiredKeyedService<IInterface1>("bazT");

        ReferenceEquals(transientFoo, transientFoo2).Should().BeFalse();
        ReferenceEquals(transientFoo, transientBar).Should().BeFalse();
        ReferenceEquals(transientFoo, transientBar).Should().BeFalse();
        ReferenceEquals(transientFoo, transientInterface).Should().BeFalse();
        ReferenceEquals(transientInterface, transientInterface2).Should().BeFalse();

        var singletonFoo = services.GetRequiredKeyedService<SingletonService1>("foo");
        var singletonFoo1 = services.GetRequiredKeyedService<SingletonService1>("foo");
        var singletonBar = services.GetRequiredKeyedService<SingletonService1>("bar");
        var singletonBaz = services.GetRequiredKeyedService<SingletonService1>("baz");
        var singletonInterface1 = services.GetRequiredKeyedService<IInterface1>("barS");
        var singletonInterface2 = services.GetRequiredKeyedService<IInterface1>("bazS");

        ReferenceEquals(singletonFoo, singletonFoo1).Should().BeTrue();
        ReferenceEquals(singletonFoo, singletonBar).Should().BeFalse();
        ReferenceEquals(singletonFoo, singletonInterface1).Should().BeFalse();
        ReferenceEquals(singletonFoo, singletonInterface2).Should().BeFalse();
        ReferenceEquals(singletonFoo, singletonBaz).Should().BeFalse();
    }
}