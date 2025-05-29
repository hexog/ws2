//HintName: Ws2.DependencyInjection.ServiceCollectionExtensions.TransientGeneric.g.cs
#nullable enable
namespace Ws2.DependencyInjection.Extensions
{
    internal static partial class ServiceCollectionExtensions
    {
        private static void AddTransientGenericServices(global::Microsoft.Extensions.DependencyInjection.IServiceCollection services)
        {
            services.Add(new global::Microsoft.Extensions.DependencyInjection.ServiceDescriptor(serviceType: typeof(global::Test.IServiceC), implementationType: typeof(global::Test.TransientService), lifetime: global::Microsoft.Extensions.DependencyInjection.ServiceLifetime.Transient));
        }
    }
}