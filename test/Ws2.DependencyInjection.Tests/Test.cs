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
            [Scoped(typeof(IServiceB))]
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
            """;

        return TestHelper.Verify(source);
    }
}