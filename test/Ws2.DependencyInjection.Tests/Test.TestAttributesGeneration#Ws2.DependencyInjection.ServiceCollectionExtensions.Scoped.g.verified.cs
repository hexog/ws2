//HintName: Ws2.DependencyInjection.ServiceCollectionExtensions.Scoped.g.cs
#nullable enable
namespace Ws2.DependencyInjection.Extensions
{
    internal static partial class ServiceCollectionExtensions
    {
        private static void AddScopedServices(global::Microsoft.Extensions.DependencyInjection.IServiceCollection services)
        {
            services.Add(new global::Microsoft.Extensions.DependencyInjection.ServiceDescriptor(serviceType: typeof(global::Test.ScopedService), implementationType: typeof(global::Test.ScopedService), lifetime: global::Microsoft.Extensions.DependencyInjection.ServiceLifetime.Scoped));
            services.Add(new global::Microsoft.Extensions.DependencyInjection.ServiceDescriptor(serviceType: typeof(global::Test.ScopedService), implementationType: typeof(global::Test.ScopedService), lifetime: global::Microsoft.Extensions.DependencyInjection.ServiceLifetime.Scoped));
            services.Add(new global::Microsoft.Extensions.DependencyInjection.ServiceDescriptor(serviceType: typeof(global::Test.ServiceE), serviceKey: 444, implementationType: typeof(global::Test.ServiceE), lifetime: global::Microsoft.Extensions.DependencyInjection.ServiceLifetime.Scoped));
        }
    }
}