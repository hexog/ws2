//HintName: Ws2.DependencyInjection.ServiceCollectionExtensions.Singleton.g.cs
#nullable enable
namespace Ws2.DependencyInjection.Extensions
{
    internal static partial class ServiceCollectionExtensions
    {
        private static void AddSingletonServices(global::Microsoft.Extensions.DependencyInjection.IServiceCollection services)
        {
            services.Add(new global::Microsoft.Extensions.DependencyInjection.ServiceDescriptor(serviceType: typeof(global::Test.ServiceD), implementationType: typeof(global::Test.ServiceD), lifetime: global::Microsoft.Extensions.DependencyInjection.ServiceLifetime.Singleton));
            services.Add(new global::Microsoft.Extensions.DependencyInjection.ServiceDescriptor(serviceType: typeof(global::Test.ServiceF), implementationType: typeof(global::Test.ServiceF), lifetime: global::Microsoft.Extensions.DependencyInjection.ServiceLifetime.Singleton));
            services.Add(new global::Microsoft.Extensions.DependencyInjection.ServiceDescriptor(serviceType: typeof(global::Test.SingletonService), implementationType: typeof(global::Test.SingletonService), lifetime: global::Microsoft.Extensions.DependencyInjection.ServiceLifetime.Singleton));
            services.Add(new global::Microsoft.Extensions.DependencyInjection.ServiceDescriptor(serviceType: typeof(global::Test.IServiceA), factory: static (provider) => global::Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService(provider, typeof(global::Test.SingletonService)), lifetime: global::Microsoft.Extensions.DependencyInjection.ServiceLifetime.Singleton));
            services.Add(new global::Microsoft.Extensions.DependencyInjection.ServiceDescriptor(serviceType: typeof(global::Test.ServiceG), implementationType: typeof(global::Test.ServiceG), lifetime: global::Microsoft.Extensions.DependencyInjection.ServiceLifetime.Singleton));
            services.Add(new global::Microsoft.Extensions.DependencyInjection.ServiceDescriptor(serviceType: typeof(global::Test.IServiceB), factory: static (provider) => global::Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService(provider, typeof(global::Test.ServiceG)), lifetime: global::Microsoft.Extensions.DependencyInjection.ServiceLifetime.Singleton));
            services.Add(new global::Microsoft.Extensions.DependencyInjection.ServiceDescriptor(serviceType: typeof(global::Test.IServiceC), implementationType: typeof(global::Test.ServiceG), lifetime: global::Microsoft.Extensions.DependencyInjection.ServiceLifetime.Singleton));
            services.Add(new global::Microsoft.Extensions.DependencyInjection.ServiceDescriptor(serviceType: typeof(global::Test.ServiceH), implementationType: typeof(global::Test.ServiceH), lifetime: global::Microsoft.Extensions.DependencyInjection.ServiceLifetime.Singleton));
            services.Add(new global::Microsoft.Extensions.DependencyInjection.ServiceDescriptor(serviceType: typeof(global::Test.IServiceA), implementationType: typeof(global::Test.ServiceH), lifetime: global::Microsoft.Extensions.DependencyInjection.ServiceLifetime.Singleton));
            services.Add(new global::Microsoft.Extensions.DependencyInjection.ServiceDescriptor(serviceType: typeof(global::Test.IServiceB), factory: static (provider) => global::Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService(provider, typeof(global::Test.ServiceH)), lifetime: global::Microsoft.Extensions.DependencyInjection.ServiceLifetime.Singleton));
            services.Add(new global::Microsoft.Extensions.DependencyInjection.ServiceDescriptor(serviceType: typeof(global::Test.IServiceC), factory: static (provider) => global::Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService(provider, typeof(global::Test.ServiceH)), lifetime: global::Microsoft.Extensions.DependencyInjection.ServiceLifetime.Singleton));
        }
    }
}