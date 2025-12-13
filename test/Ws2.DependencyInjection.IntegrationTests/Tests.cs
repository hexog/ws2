using Microsoft.Extensions.DependencyInjection;
using Ws2.DependencyInjection.Extensions;

namespace Ws2.DependencyInjection.IntegrationTests;

public interface IService;

[Transient<IService>]
public class Implementation : IService;

public interface IAbstractServiceA;

public interface IAbstractServiceB;

[Singleton<IAbstractServiceA>(Shared = true)]
[Singleton(ServiceType = typeof(IAbstractServiceB), Shared = true)]
public class AbstractServiceImplementation : IAbstractServiceA, IAbstractServiceB;

public interface ISeparateAbstractServiceA;

public interface ISeparateAbstractServiceB;

[Singleton<ISeparateAbstractServiceA>]
[Singleton<ISeparateAbstractServiceB>]
public class SeparateAbstractServiceImplementation : ISeparateAbstractServiceA, ISeparateAbstractServiceB;

public interface IComplexSingletonA;

public interface IComplexSingletonB;

public interface IComplexSingletonC;

[Singleton<IComplexSingletonA>]
[Singleton<IComplexSingletonB>(Shared = true)]
[Singleton<IComplexSingletonC>(Shared = true)]
public class ComplexSingleton : IComplexSingletonA, IComplexSingletonB,  IComplexSingletonC;

[Singleton]
[Singleton(ServiceKey = 4)]
public class KeyedSingleton;

[Singleton(Shared = true)]
[Singleton(ServiceKey = 4, Shared = true)]
public class KeyedSharedSingleton;

public class Tests
{
    private ServiceProvider provider = null!;

    [SetUp]
    public void Setup()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddWs2DependencyInjectionIntegrationTests();
        provider = serviceCollection.BuildServiceProvider();
    }

    [TearDown]
    public void TearDown()
    {
        provider.Dispose();
    }

    [Test]
    public void TestResolve()
    {
        var service = provider.GetService<IService>();

        Assert.Multiple(() =>
        {
            Assert.That(service, Is.Not.Null);
            Assert.That(service, Is.InstanceOf<Implementation>());
        });
    }

    [Test]
    public void TestMultipleInstanceOfSingleton()
    {
        var a = provider.GetRequiredService<IAbstractServiceA>();
        var b = provider.GetRequiredService<IAbstractServiceB>();

        Assert.Multiple(() =>
        {
            Assert.That(a, Is.InstanceOf<AbstractServiceImplementation>());
            Assert.That(b, Is.InstanceOf<AbstractServiceImplementation>());
            Assert.That(ReferenceEquals(a, b));
        });
    }

    [Test]
    public void TestMultipleInstanceOfSingletonSeparate()
    {
        var a = provider.GetRequiredService<ISeparateAbstractServiceA>();
        var b = provider.GetRequiredService<ISeparateAbstractServiceB>();

        Assert.Multiple(() =>
        {
            Assert.That(a, Is.InstanceOf<SeparateAbstractServiceImplementation>());
            Assert.That(b, Is.InstanceOf<SeparateAbstractServiceImplementation>());
            Assert.That(ReferenceEquals(a, b), Is.False);
        });
    }

    [Test]
    public void TestComplexSingletonScenario()
    {
        var a = provider.GetRequiredService<IComplexSingletonA>();
        var b = provider.GetRequiredService<IComplexSingletonB>();
        var c = provider.GetRequiredService<IComplexSingletonB>();

        Assert.Multiple(() =>
        {
            Assert.That(a, Is.InstanceOf<ComplexSingleton>());
            Assert.That(b, Is.InstanceOf<ComplexSingleton>());
            Assert.That(c, Is.InstanceOf<ComplexSingleton>());
            Assert.That(ReferenceEquals(a, b), Is.False);
            Assert.That(ReferenceEquals(b, c), Is.True);
        });
    }

    [Test]
    public void TestKeyedSingleton()
    {
        var a = provider.GetRequiredService<KeyedSingleton>();
        var b = provider.GetRequiredKeyedService<KeyedSingleton>(4);

        Assert.That(ReferenceEquals(a, b), Is.False);
    }

    [Test]
    public void TestKeyedSharedSingleton()
    {
        var a = provider.GetRequiredService<KeyedSharedSingleton>();
        var b = provider.GetRequiredKeyedService<KeyedSharedSingleton>(4);

        Assert.That(ReferenceEquals(a, b), Is.True);
    }
}