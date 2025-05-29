using Microsoft.Extensions.DependencyInjection;
using Ws2.DependencyInjection.Extensions;

namespace Ws2.DependencyInjection.IntegrationTests;

public interface IService;

[Transient<IService>]
public class Implementation : IService;

public interface IAbstractServiceA;

public interface IAbstractServiceB;

[Singleton<IAbstractServiceA>]
[Singleton(ServiceType = typeof(IAbstractServiceB))]
public class AbstractServiceImplementation : IAbstractServiceA, IAbstractServiceB;

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
}