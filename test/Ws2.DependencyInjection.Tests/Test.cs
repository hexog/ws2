namespace Ws2.DependencyInjection.Tests;

public class Test
{
    [Test]
    public Task TestAttributesGeneration()
    {
        const string source =//lang=cs
            """
            using Ws2.DependencyInjection;

            namespace Test;

            public interface IServiceA;

            [Singleton<IServiceA>]
            public class SingletonService : IServiceA;
            
            public interface IServiceB;
            public interface IServiceC
            
            [Scoped]
            [Scoped(ServiceType = typeof(IServiceB))]
            public class ScopedService : IServiceB, IServiceC;
            
            internal interface IServiceC;
            
            [Transient<IServiceC>]
            internal class TransientService : IServiceC;
            
            [Singleton]
            internal class ServiceD;
            
            [Scoped(ServiceKey = 444.000)]
            public class ServiceE;
            
            [Singleton]
            [Singleton(ServiceKey = "ff")]
            public class ServiceF : ServiceD;
            
            [Singleton]
            [Singleton(ServiceType = typeof(ServiceD))]
            public class ServiceF1 : ServiceD;
            
            [Singleton(Shared = true)]
            [Singleton(ServiceType = typeof(ServiceD), Shared = true)]
            public class ServiceF2 : ServiceD;
            
            [Singleton<IServiceB>]
            [Singleton<IServiceC>(Shared = true)]
            public class ServiceG : IServiceB, IServiceC;
            
            [Singleton<IServiceA>]
            [Singleton<IServiceB>]
            [Singleton<IServiceC>]
            public class ServiceH : IServiceA, IServiceB, IServiceC;
            """;

        return TestHelper.Verify(source);
    }
}