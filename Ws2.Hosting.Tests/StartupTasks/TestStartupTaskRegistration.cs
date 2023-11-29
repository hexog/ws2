using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NSubstitute;
using Ws2.Hosting.StartupTasks;

namespace Ws2.Hosting.Tests.StartupTasks;

public class TestStartupTaskRegistration
{
    [Test]
    public void TestSingleStartupTaskRegistration()
    {
        var startupTask = Substitute.For<IStartupTask>();

        var builder = Host.CreateApplicationBuilder();
        builder.Services.AddStartupTask(startupTask);
        var app = builder.Build();

        startupTask.Priority.Returns(10);

        app.ExecuteStartupTasks();

        startupTask.Received()
            .ExecuteAsync(Arg.Any<CancellationToken>());
    }

    [Test]
    public void TestSingleStartupTaskRegistrationFromAssembly()
    {
        var builder = Host.CreateApplicationBuilder();

        builder.Services.AddServicesByAttributesFromTypes(new[]
        {
            typeof(StartupTaskImplementationRegistrar),
            typeof(TestStartupTask)
        });

        var app = builder.Build();

        app.ExecuteStartupTasks();

        TestStartupTask.Invoked.Should().BeTrue();
    }

    public class TestStartupTask : IStartupTask
    {
        public static bool Invoked { get; private set; }

        public ValueTask ExecuteAsync(CancellationToken cancellationToken)
        {
            Invoked = true;
            return ValueTask.CompletedTask;
        }

        public int Priority => 15;
    }

}