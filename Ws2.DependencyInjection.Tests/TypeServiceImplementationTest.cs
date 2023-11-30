using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Ws2.DependencyInjection.Abstractions;

namespace Ws2.DependencyInjection.Tests;

public class TypeServiceImplementationTest
{
    private static IServiceProvider CreateServiceProvider()
    {
        var serviceCollection = new ServiceCollection();

        serviceCollection.AddServicesByAttributesFromTypes(new[]
        {
            typeof(IMyInterface),
            typeof(MyInterfaceImplementation),
            typeof(MyInterfaceRegistrar),
            typeof(UnusedType),
            typeof(MyBaseType),
            typeof(MyInheritedType),
            typeof(MyBaseTypeRegistrar)
        });

        return serviceCollection.BuildServiceProvider();
    }

    [Test]
    public void TestRegisterInterfaceImplementation()
    {
        using var serviceScope = CreateServiceProvider().CreateScope();
        var myInterface = serviceScope.ServiceProvider.GetService<IMyInterface>();
        myInterface.Should().NotBeNull();
        myInterface.Should().BeOfType<MyInterfaceImplementation>();
    }

    [Test]
    public void TestRegisterBaseClassImplementation()
    {
        using var serviceScope = CreateServiceProvider().CreateScope();
        var myInterface = serviceScope.ServiceProvider.GetService<MyBaseType>();
        myInterface.Should().NotBeNull();
        myInterface.Should().BeOfType<MyInheritedType>();
    }

    [Test]
    public void TestRegisterUnusedClassReturnsNothing()
    {
        using var serviceScope = CreateServiceProvider().CreateScope();
        var myInterface = serviceScope.ServiceProvider.GetService<UnusedType>();
        myInterface.Should().BeNull();
    }
}

public interface IMyInterface
{
}

public class MyInterfaceImplementation : IMyInterface
{
}

public class MyInterfaceRegistrar : IServiceTypeImplementationRegistrar<IMyInterface>
{
    public void Register(IServiceAttributeRegistrarContext context, Type type)
    {
        context.ServiceCollection.AddScoped(typeof(IMyInterface), type);
    }
}

public class UnusedType
{
}

public abstract class MyBaseType
{
}

public class MyInheritedType : MyBaseType
{
}

public class MyBaseTypeRegistrar : IServiceTypeImplementationRegistrar<MyBaseType>
{
    public void Register(IServiceAttributeRegistrarContext context, Type type)
    {
        context.ServiceCollection.AddScoped(typeof(MyBaseType), type);
    }
}