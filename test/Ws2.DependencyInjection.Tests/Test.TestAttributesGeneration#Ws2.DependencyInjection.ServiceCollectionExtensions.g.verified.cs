//HintName: Ws2.DependencyInjection.ServiceCollectionExtensions.g.cs
#nullable enable
namespace Ws2.DependencyInjection.Extensions
{
    internal static partial class ServiceCollectionExtensions
    {
        public static global::Microsoft.Extensions.DependencyInjection.IServiceCollection AddTests(this global::Microsoft.Extensions.DependencyInjection.IServiceCollection services)
        {
            AddSingletonServices(services);
            AddScopedServices(services);
            AddScopedGenericServices(services);
            AddTransientServices(services);
            AddTransientGenericServices(services);
            return services;
        }
    }
}