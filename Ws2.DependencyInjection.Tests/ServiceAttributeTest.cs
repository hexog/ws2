using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace Ws2.DependencyInjection.Tests;

public class ServiceAttributeTest
{
    private ServiceProvider serviceProvider = null!;
    private FrozenDictionary<string, Type> typeNameToType = null!;
    private ServiceCollection serviceCollection = null!;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        serviceCollection = new ServiceCollection();
        var (testAssembly, assemblyTypes) = CreateTestAssembly(GoodTypeDescriptions);
        serviceCollection.AddServicesByAttributes(testAssembly);
        typeNameToType = assemblyTypes;
        serviceProvider = serviceCollection.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateScopes = true,
            ValidateOnBuild = true
        });
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        serviceProvider.Dispose();
    }

    private static IEnumerable<TypeDescription> GoodTypeDescriptions
    {
        get
        {
            yield return new TypeDescription("SimpleService", ServiceLifetime.Scoped);

            yield return new TypeDescription("SimpleInterface", ServiceLifetime.Scoped, "ISimpleInterface");

            yield return new TypeDescription("SimpleTwoInterfaces")
                .AddScopedService("ISimpleTwoInterfacesA")
                .AddScopedService("ISimpleTwoInterfaces1");

            yield return new TypeDescription("SimpleTransient", ServiceLifetime.Transient);

            yield return new TypeDescription("SingletonService", ServiceLifetime.Singleton);

            yield return new TypeDescription("SingletonMultipleInterfaces")
                .AddSingletonService("ISingletonMultipleInterfacesA")
                .AddSingletonService("ISingletonMultipleInterfaces1");

            yield return new TypeDescription("SingletonWithOwnInstance")
                .AddSingletonService("ISingletonWithOwnInstanceA", ServiceType.Interface,
                    SingletonServiceInstanceSharing.OwnInstance)
                .AddSingletonService("ISingletonWithOwnInstance1", ServiceType.Interface,
                    SingletonServiceInstanceSharing.OwnInstance);

            yield return new TypeDescription("MixedService")
                .AddScopedService("ScopedMixedService")
                .AddTransientService("TransientMixedService")
                .AddSingletonService("SingletonMixedService");
        }
    }

    [Test]
    public void TestGetSimpleService()
    {
        using var serviceScope = serviceProvider.CreateScope();
        var typeInstance = serviceScope.ServiceProvider.GetService(typeNameToType["SimpleService"]);
        typeInstance.Should().NotBeNull();
    }

    [Test]
    public void TestGetSimpleServiceByInterface()
    {
        using var serviceScope = serviceProvider.CreateScope();
        var typeInstance = serviceScope.ServiceProvider.GetService(typeNameToType["ISimpleInterface"]);
        typeInstance.Should().NotBeNull();
    }

    [Test]
    public void TestGetSameImplementationByMultipleServices()
    {
        using var serviceScope = serviceProvider.CreateScope();
        var typeInstanceA = serviceScope.ServiceProvider.GetService(typeNameToType["ISimpleTwoInterfacesA"]);
        var typeInstance1 = serviceScope.ServiceProvider.GetService(typeNameToType["ISimpleTwoInterfaces1"]);

        typeInstance1.Should().NotBeNull();
        typeInstance1.Should().NotBeNull();
        typeInstanceA.Should().BeOfType(typeInstance1!.GetType(), "Same type was registered as multiple services");
    }

    [Test]
    public void TestGetSingletonServiceReturnsSameInstance()
    {
        var typeInstance = serviceProvider.GetService(typeNameToType["SingletonService"]);
        typeInstance.Should().NotBeNull();
        var anotherTypeInstance = serviceProvider.GetService(typeNameToType["SingletonService"]);
        anotherTypeInstance.Should().NotBeNull();

        ReferenceEquals(typeInstance, anotherTypeInstance).Should()
            .BeTrue("Singleton returns same type instance");
    }

    [Test]
    public void TestGetSingletonServiceMultipleInterfacesReturnsSameInstance()
    {
        var typeInstanceA = serviceProvider.GetService(typeNameToType["ISingletonMultipleInterfacesA"]);
        var typeInstance1 = serviceProvider.GetService(typeNameToType["ISingletonMultipleInterfaces1"]);
        typeInstanceA.Should().NotBeNull();
        typeInstance1.Should().NotBeNull();
        ReferenceEquals(typeInstanceA, typeInstance1).Should().BeTrue("Default singleton returns same type instance");
    }

    [Test]
    public void TestGetSingletonServiceMultipleInterfacesWithOwnInstanceReturnsDifferentInstance()
    {
        var typeInstanceA = serviceProvider.GetService(typeNameToType["ISingletonWithOwnInstanceA"]);
        var typeInstance1 = serviceProvider.GetService(typeNameToType["ISingletonWithOwnInstance1"]);
        typeInstanceA.Should().NotBeNull();
        typeInstance1.Should().NotBeNull();
        ReferenceEquals(typeInstanceA, typeInstance1).Should().BeFalse("Default singleton returns same type instance");
    }

    [Test]
    public void TestSimpleTransientService()
    {
        using var serviceScope = serviceProvider.CreateScope();
        var typeInstanceA = serviceScope.ServiceProvider.GetService(typeNameToType["SimpleTransient"]);
        typeInstanceA.Should().NotBeNull();
        var typeInstanceB = serviceScope.ServiceProvider.GetService(typeNameToType["SimpleTransient"]);
        typeInstanceB.Should().NotBeNull();
        ReferenceEquals(typeInstanceA, typeInstanceB).Should().BeFalse();
    }

    [Test]
    public void TestRegisterMixedServicesType()
    {
        using var serviceScope = serviceProvider.CreateScope();
        var scoped = serviceScope.ServiceProvider.GetService(typeNameToType["ScopedMixedService"]);
        scoped.Should().NotBeNull();

        var transient = serviceScope.ServiceProvider.GetService(typeNameToType["TransientMixedService"]);
        transient.Should().NotBeNull();
        transient.Should().BeOfType(scoped!.GetType());
        ReferenceEquals(scoped, transient).Should().BeFalse();

        var singleton = serviceScope.ServiceProvider.GetService(typeNameToType["SingletonMixedService"]);
        singleton.Should().NotBeNull();
        singleton.Should().BeOfType(scoped!.GetType());
        ReferenceEquals(scoped, singleton).Should().BeFalse();
    }

    #region Test assembly creation

    private const TypeAttributes DefaultTypeAttributes = TypeAttributes.Public | TypeAttributes.AutoClass;

    private static (Assembly Assembly, FrozenDictionary<string, Type> AssemblyTypes) CreateTestAssembly(
        IEnumerable<TypeDescription> typesDescription
    )
    {
        const string testAssemblyName = "DynamicTestAssembly";

        var assemblyTypes = new Dictionary<string, Type>();
        var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(
            new AssemblyName { Name = testAssemblyName },
            AssemblyBuilderAccess.Run
        );

        var moduleBuilder = assemblyBuilder.DefineDynamicModule(testAssemblyName);
        foreach (var typeDescription in typesDescription)
        {
            var typeBuilder =
                moduleBuilder.DefineType(typeDescription.TypeName, DefaultTypeAttributes | TypeAttributes.Class);
            foreach (var service in typeDescription.Services)
            {
                if (service.ServiceName is null)
                {
                    typeBuilder.SetCustomAttribute(
                        GetServiceAttributeCustomAttributeBuilder(service.ServiceLifetime, null, null)
                    );
                }
                else
                {
                    var serviceTypeAttributes = service.ServiceType switch
                    {
                        ServiceType.Interface => TypeAttributes.Interface
                            | TypeAttributes.Abstract
                            | TypeAttributes.AutoClass,
                        ServiceType.AbstractClass => TypeAttributes.Class
                            | TypeAttributes.Abstract
                            | TypeAttributes.Abstract,
                        ServiceType.Class => TypeAttributes.Class | TypeAttributes.AutoClass,
                        _ => throw new ArgumentOutOfRangeException()
                    };

                    var serviceType = moduleBuilder
                        .DefineType(service.ServiceName, serviceTypeAttributes | DefaultTypeAttributes)
                        .CreateType();

                    switch (service.ServiceType)
                    {
                        case ServiceType.Interface:
                            typeBuilder.AddInterfaceImplementation(serviceType);
                            break;
                        case ServiceType.AbstractClass or ServiceType.Class:
                            typeBuilder.SetParent(serviceType);
                            break;
                    }

                    assemblyTypes[service.ServiceName] = serviceType;
                    typeBuilder.SetCustomAttribute(
                        GetServiceAttributeCustomAttributeBuilder(
                            service.ServiceLifetime,
                            serviceType, service.SingletonInstanceSharing
                        )
                    );
                }
            }

            assemblyTypes[typeDescription.TypeName] = typeBuilder.CreateType();
        }

        return (assemblyBuilder, assemblyTypes.ToFrozenDictionary());
    }

    private static CustomAttributeBuilder GetServiceAttributeCustomAttributeBuilder(
        ServiceLifetime serviceLifetime,
        Type? serviceType,
        SingletonServiceInstanceSharing? instanceSharing
    )
    {
        return serviceLifetime switch
        {
            ServiceLifetime.Singleton =>
                CreateCustomAttributeBuilder(typeof(SingletonServiceAttribute), serviceType, instanceSharing),
            ServiceLifetime.Scoped =>
                CreateCustomAttributeBuilder(typeof(ScopedServiceAttribute), serviceType, instanceSharing),
            ServiceLifetime.Transient =>
                CreateCustomAttributeBuilder(typeof(TransientServiceAttribute), serviceType, instanceSharing),
            _ => throw new ArgumentOutOfRangeException(nameof(serviceLifetime), serviceLifetime, null)
        };

        static CustomAttributeBuilder CreateCustomAttributeBuilder(Type serviceAttributeType, Type? serviceType,
            SingletonServiceInstanceSharing? instanceSharing)
        {
            if (serviceType is null)
            {
                var constructorInfo = serviceAttributeType.GetConstructor(Array.Empty<Type>());
                Debug.Assert(constructorInfo is not null);
                return new CustomAttributeBuilder(
                    constructorInfo, Array.Empty<object?>()
                );
            }
            else
            {
                var constructorInfo = serviceAttributeType.GetConstructor(new[] { typeof(string) });
                Debug.Assert(constructorInfo is not null);

                if (typeof(SingletonServiceAttribute).IsAssignableFrom(serviceAttributeType))
                {
                    var propertyInfo = serviceAttributeType.GetProperty("InstanceSharing");
                    Debug.Assert(propertyInfo is not null);
                    return new CustomAttributeBuilder(
                        constructorInfo,
                        new object?[] { serviceType.FullName },
                        new[] { propertyInfo },
                        new object?[] { instanceSharing }
                    );
                }

                return new CustomAttributeBuilder(
                    constructorInfo,
                    new object?[] { serviceType.FullName }
                );
            }
        }
    }

    public enum ServiceType
    {
        Interface = 1,
        AbstractClass = 2,
        Class = 3
    }

    public readonly record struct TypeDescription(
        string TypeName,
        IImmutableList<(string? ServiceName, ServiceLifetime ServiceLifetime, ServiceType? ServiceType,
            SingletonServiceInstanceSharing? SingletonInstanceSharing)> Services
    )
    {
        public TypeDescription(string typeName)
            : this(typeName,
                ImmutableList<(string?, ServiceLifetime, ServiceType?, SingletonServiceInstanceSharing?)>.Empty)
        {
        }

        public TypeDescription(
            string typeName,
            ServiceLifetime serviceLifetime
        ) : this(
            typeName,
            ImmutableList.Create((
                (string?)null,
                serviceLifetime,
                (ServiceType?)null,
                (SingletonServiceInstanceSharing?)null
            ))
        )
        {
        }

        public TypeDescription(
            string typeName,
            ServiceLifetime serviceLifetime,
            string serviceName,
            ServiceType serviceType = ServiceType.Interface
        ) : this(
            typeName,
            ImmutableList.Create((
                (string?)serviceName,
                serviceLifetime,
                (ServiceType?)serviceType,
                (SingletonServiceInstanceSharing?)null
            ))
        )
        {
        }

        public TypeDescription AddService(ServiceLifetime serviceLifetime)
        {
            return this with { Services = Services.Add((null, serviceLifetime, null, null)) };
        }

        public TypeDescription AddScopedService()
        {
            return this with { Services = Services.Add((null, ServiceLifetime.Scoped, null, null)) };
        }

        public TypeDescription AddSingletonService()
        {
            return this with { Services = Services.Add((null, ServiceLifetime.Singleton, null, null)) };
        }

        public TypeDescription AddTransientService()
        {
            return this with { Services = Services.Add((null, ServiceLifetime.Transient, null, null)) };
        }


        public TypeDescription AddService(
            string serviceName,
            ServiceLifetime serviceLifetime,
            ServiceType type = ServiceType.Interface
        )
        {
            return this with { Services = Services.Add((serviceName, serviceLifetime, type, null)) };
        }

        public TypeDescription AddScopedService(
            string serviceName,
            ServiceType type = ServiceType.Interface
        )
        {
            return this with { Services = Services.Add((serviceName, ServiceLifetime.Scoped, type, null)) };
        }

        public TypeDescription AddSingletonService(
            string serviceName,
            ServiceType type = ServiceType.Interface,
            SingletonServiceInstanceSharing instanceSharing = SingletonServiceInstanceSharing.SharedInstance
        )
        {
            return this with
            {
                Services = Services.Add((serviceName, ServiceLifetime.Singleton, type, instanceSharing))
            };
        }

        public TypeDescription AddTransientService(
            string serviceName,
            ServiceType type = ServiceType.Interface
        )
        {
            return this with { Services = Services.Add((serviceName, ServiceLifetime.Transient, type, null)) };
        }
    }

    #endregion
}