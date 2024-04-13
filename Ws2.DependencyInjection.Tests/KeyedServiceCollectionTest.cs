using Microsoft.Extensions.DependencyInjection;
using Ws2.DependencyInjection.Abstractions;

namespace Ws2.DependencyInjection.Tests;

public class KeyedServiceCollectionTest
{
    private const string ServiceKey = "MyKey";

    private IServiceCollection serviceCollection = default!;

    [SetUp]
    public void SetUp()
    {
        serviceCollection = new ServiceCollection();

        serviceCollection.AddKeyedSingleton<string>(ServiceKey, "Service");
    }

    [Test]
    public void TestRegisterOnCollectionWithKeyedServicesDoesNotThrow()
    {
        Assert.DoesNotThrow(
            () => serviceCollection.AddServicesFromTypes(Array.Empty<Type>(), Array.Empty<IServiceRegistrar>())
        );
    }
}