using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Ws2.DependencyInjection.Abstractions;
using Ws2.Hosting.Registrars;

namespace Ws2.Hosting.Tests;

public class HostedServiceRegistrarTest
{
    public class HostedService1 : IHostedService
    {
        public Task StartAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }

    public class HostedService2 : BackgroundService
    {
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return Task.CompletedTask;
        }
    }

    public abstract class AbstractHostedService : BackgroundService
    {
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return Task.CompletedTask;
        }
    }

    public class OtherClass
    {
    }

    [Test]
    public void TestRegisterHostedServices()
    {
        var services = new ServiceCollection();

        services.AddServicesByAttributesFromTypes(
            new[] { typeof(HostedService1), typeof(HostedService2), typeof(AbstractHostedService), typeof(OtherClass) },
            new IServiceRegistrar[] { new HostedServiceRegistrar() }
        );

        var serviceDescriptors = services.ToList();
        serviceDescriptors.Count.Should().Be(2);
        serviceDescriptors[0].ImplementationType.Should().Be(typeof(HostedService1));
        serviceDescriptors[1].ImplementationType.Should().Be(typeof(HostedService2));
    }
}