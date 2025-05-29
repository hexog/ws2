using System.Collections.Immutable;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Ws2.DependencyInjection;

[Generator]
public class ServiceCollectionExtensionsGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(static ctx => ctx.AddSource(
            "Ws2.DependencyInjection.ServiceLifetimeAttributes.g.cs",
            SourceText.From(
                //lang=cs
                """
                #nullable enable
                namespace Ws2.DependencyInjection
                {
                    [global::System.AttributeUsage(global::System.AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
                    [global::System.Diagnostics.Conditional("WS2_DI_USAGES")]
                    internal sealed class SingletonAttribute : global::System.Attribute
                    {
                        public global::System.Type? ServiceType { get; set; }

                        public object? ServiceKey { get; set; }
                    }

                    [global::System.AttributeUsage(global::System.AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
                    [global::System.Diagnostics.Conditional("WS2_DI_USAGES")]
                    internal sealed class SingletonAttribute<TService> : global::System.Attribute
                    {
                        public object? ServiceKey { get; set; }
                    }

                    [global::System.AttributeUsage(global::System.AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
                    [global::System.Diagnostics.Conditional("WS2_DI_USAGES")]
                    internal sealed class ScopedAttribute : global::System.Attribute
                    {
                        public global::System.Type? ServiceType { get; set; }

                        public object? ServiceKey { get; set; }
                    }

                    [global::System.AttributeUsage(global::System.AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
                    [global::System.Diagnostics.Conditional("WS2_DI_USAGES")]
                    internal sealed class ScopedAttribute<TService> : global::System.Attribute
                    {
                        public object? ServiceKey { get; set; }
                    }

                    [global::System.AttributeUsage(global::System.AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
                    [global::System.Diagnostics.Conditional("WS2_DI_USAGES")]
                    internal sealed class TransientAttribute : global::System.Attribute
                    {
                        public global::System.Type? ServiceType { get; set; }

                        public object? ServiceKey { get; set; }
                    }

                    [global::System.AttributeUsage(global::System.AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
                    [global::System.Diagnostics.Conditional("WS2_DI_USAGES")]
                    internal sealed class TransientAttribute<TService> : global::System.Attribute
                    {
                        public object? ServiceKey { get; set; }
                    }
                }
                """,
                Encoding.UTF8
            )
        ));

        var singletonDescriptors = context.SyntaxProvider.ForAttributeWithMetadataName(
            "Ws2.DependencyInjection.SingletonAttribute",
            predicate: static (_, _) => true,
            transform: static (ctx, _) => GetDescriptors(ctx, ServiceLifetime.Singleton)
        ).Where(x => !x.IsEmpty).SelectMany((array, _) => array.Array ?? []);

        var singletonGenericDescriptors = context.SyntaxProvider.ForAttributeWithMetadataName(
            "Ws2.DependencyInjection.SingletonAttribute`1",
            predicate: static (_, _) => true,
            transform: static (ctx, _) => GetDescriptors(ctx, ServiceLifetime.Singleton)
        ).Where(x => !x.IsEmpty).SelectMany((array, _) => array.Array ?? []);

        var scopedDescriptors = context.SyntaxProvider.ForAttributeWithMetadataName(
            "Ws2.DependencyInjection.ScopedAttribute",
            predicate: static (_, _) => true,
            transform: static (ctx, _) => GetDescriptors(ctx, ServiceLifetime.Scoped)
        ).Where(x => !x.IsEmpty).SelectMany((array, _) => array.Array ?? []);

        var scopedGenericDescriptors = context.SyntaxProvider.ForAttributeWithMetadataName(
            "Ws2.DependencyInjection.ScopedAttribute`1",
            predicate: static (_, _) => true,
            transform: static (ctx, _) => GetDescriptors(ctx, ServiceLifetime.Scoped)
        ).Where(x => !x.IsEmpty).SelectMany((array, _) => array.Array ?? []);

        var transientDescriptors = context.SyntaxProvider.ForAttributeWithMetadataName(
            "Ws2.DependencyInjection.TransientAttribute",
            predicate: static (_, _) => true,
            transform: static (ctx, _) => GetDescriptors(ctx, ServiceLifetime.Transient)
        ).Where(x => !x.IsEmpty).SelectMany((array, _) => array.Array ?? []);

        var transientGenericDescriptors = context.SyntaxProvider.ForAttributeWithMetadataName(
            "Ws2.DependencyInjection.TransientAttribute`1",
            predicate: static (_, _) => true,
            transform: static (ctx, _) => GetDescriptors(ctx, ServiceLifetime.Transient)
        ).Where(x => !x.IsEmpty).SelectMany((array, _) => array.Array ?? []);

        var assemblyNameValueProvider = context.CompilationProvider
            .Select(static (compilation, _) =>
                {
                    var assemblyName = compilation.AssemblyName;
                    return assemblyName ?? "Services";
                }
            );

        context.RegisterSourceOutput(singletonDescriptors.Collect().Combine(singletonGenericDescriptors.Collect()),
            static (ctx, descriptor) => WriteAddSingletonDescriptors(ctx, descriptor, "Singleton"));
        context.RegisterSourceOutput(scopedDescriptors.Collect(),
            static (ctx, descriptor) => WriteAddDescriptors(ctx, descriptor, "Scoped"));
        context.RegisterSourceOutput(scopedGenericDescriptors.Collect(),
            static (ctx, descriptor) => WriteAddDescriptors(ctx, descriptor, "ScopedGeneric"));
        context.RegisterSourceOutput(transientDescriptors.Collect(),
            static (ctx, descriptor) => WriteAddDescriptors(ctx, descriptor, "Transient"));
        context.RegisterSourceOutput(transientGenericDescriptors.Collect(),
            static (ctx, descriptor) => WriteAddDescriptors(ctx, descriptor, "TransientGeneric"));

        context.RegisterSourceOutput(assemblyNameValueProvider, static (ctx, assemblyName) => ctx.AddSource(
            "Ws2.DependencyInjection.ServiceCollectionExtensions.g.cs",
            SourceText.From(
                //lang=cs
                $$"""
                  #nullable enable
                  namespace Ws2.DependencyInjection.Extensions
                  {
                      internal static partial class ServiceCollectionExtensions
                      {
                          public static global::Microsoft.Extensions.DependencyInjection.IServiceCollection Add{{assemblyName.Replace(".", "")}}(this global::Microsoft.Extensions.DependencyInjection.IServiceCollection services)
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
                  """,
                Encoding.UTF8
            )
        ));
    }

    private static void WriteAddSingletonDescriptors(
        SourceProductionContext context,
        (ImmutableArray<ValueServiceDescriptor> Left, ImmutableArray<ValueServiceDescriptor> Right) allDescriptors,
        string marker
    )
    {
        var registeredSingletons = new HashSet<RegisteredSingleton>();
        var sb = new StringBuilder();
        foreach (var (implementationTypeName, serviceKey, serviceTypeName, lifetime) in allDescriptors.Left)
        {
            if (lifetime != ServiceLifetime.Singleton)
            {
                continue;
            }

            if (registeredSingletons.Add(new RegisteredSingleton(implementationTypeName, serviceKey)))
            {
                sb.Append("            services.Add(new global::Microsoft.Extensions.DependencyInjection.ServiceDescriptor(");
                sb.Append("serviceType: typeof(global::").Append(implementationTypeName).Append(')');
                if (serviceKey is not null)
                {
                    sb.Append(", serviceKey: ").Append(Convert.ToString(serviceKey, CultureInfo.InvariantCulture));
                }

                sb.Append(", implementationType: typeof(global::").Append(implementationTypeName).Append(')');
                sb.AppendLine(", lifetime: global::Microsoft.Extensions.DependencyInjection.ServiceLifetime.Singleton));");
            }

            if (serviceTypeName is not null && implementationTypeName != serviceTypeName)
            {
                sb.Append("            services.Add(new global::Microsoft.Extensions.DependencyInjection.ServiceDescriptor(");
                sb.Append("serviceType: typeof(global::").Append(serviceTypeName).Append(')');
                if (serviceKey is not null)
                {
                    sb.Append(", serviceKey: ").Append(Convert.ToString(serviceKey, CultureInfo.InvariantCulture));
                    sb.Append(
                            ", factory: static (provider, key) => global::Microsoft.Extensions.DependencyInjection.ServiceProviderKeyedServiceExtensions.GetRequiredKeyedService(provider, typeof(global::")
                        .Append(implementationTypeName).Append("), key)");
                }
                else
                {
                    sb.Append(
                            ", factory: static (provider) => global::Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService(provider, typeof(global::")
                        .Append(implementationTypeName).Append("))");
                }

                sb.Append(", lifetime: global::Microsoft.Extensions.DependencyInjection.ServiceLifetime.")
                    .Append(GetServiceLifetimeName(lifetime)).Append("));");
                sb.AppendLine();
            }
        }

        foreach (var (implementationTypeName, serviceKey, serviceTypeName, lifetime) in allDescriptors.Right)
        {
            if (lifetime != ServiceLifetime.Singleton)
            {
                continue;
            }

            if (registeredSingletons.Add(new RegisteredSingleton(implementationTypeName, serviceKey)))
            {
                sb.Append("            services.Add(new global::Microsoft.Extensions.DependencyInjection.ServiceDescriptor(");
                sb.Append("serviceType: typeof(global::").Append(implementationTypeName).Append(')');
                if (serviceKey is not null)
                {
                    sb.Append(", serviceKey: ").Append(Convert.ToString(serviceKey, CultureInfo.InvariantCulture));
                }

                sb.Append(", implementationType: typeof(global::").Append(implementationTypeName).Append(')');
                sb.AppendLine(", lifetime: global::Microsoft.Extensions.DependencyInjection.ServiceLifetime.Singleton));");
            }

            if (serviceTypeName is not null && implementationTypeName != serviceTypeName)
            {
                sb.Append("            services.Add(new global::Microsoft.Extensions.DependencyInjection.ServiceDescriptor(");
                sb.Append("serviceType: typeof(global::").Append(serviceTypeName).Append(')');
                if (serviceKey is not null)
                {
                    sb.Append(", serviceKey: ").Append(Convert.ToString(serviceKey, CultureInfo.InvariantCulture));
                    sb.Append(
                            ", factory: static (provider, key) => global::Microsoft.Extensions.DependencyInjection.ServiceProviderKeyedServiceExtensions.GetRequiredKeyedService(provider, typeof(global::")
                        .Append(implementationTypeName).Append("), key)");
                }
                else
                {
                    sb.Append(
                            ", factory: static (provider) => global::Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService(provider, typeof(global::")
                        .Append(implementationTypeName).Append("))");
                }

                sb.Append(", lifetime: global::Microsoft.Extensions.DependencyInjection.ServiceLifetime.")
                    .Append(GetServiceLifetimeName(lifetime)).Append("));");
                sb.AppendLine();
            }
        }

        context.AddSource(
            $"Ws2.DependencyInjection.ServiceCollectionExtensions.{marker}.g.cs",
            SourceText.From(
                //lang=cs
                $$"""
                  #nullable enable
                  namespace Ws2.DependencyInjection.Extensions
                  {
                      internal static partial class ServiceCollectionExtensions
                      {
                          private static void Add{{marker}}Services(global::Microsoft.Extensions.DependencyInjection.IServiceCollection services)
                          {
                  {{sb}}        }
                      }
                  }
                  """,
                Encoding.UTF8)
        );
    }

    private static void WriteAddDescriptors(
        SourceProductionContext context,
        ImmutableArray<ValueServiceDescriptor> descriptors,
        string marker
    )
    {
        var sb = new StringBuilder();
        foreach (var (implementationTypeName, serviceKey, serviceTypeName, lifetime) in descriptors)
        {
            sb.Append("            services.Add(new global::Microsoft.Extensions.DependencyInjection.ServiceDescriptor(");
            sb.Append("serviceType: typeof(global::").Append(serviceTypeName ?? implementationTypeName).Append(')');
            if (serviceKey is not null)
            {
                sb.Append(", serviceKey: ").Append(Convert.ToString(serviceKey, CultureInfo.InvariantCulture));
            }

            sb.Append(", implementationType: typeof(global::").Append(implementationTypeName).Append(')');
            sb.Append(", lifetime: global::Microsoft.Extensions.DependencyInjection.ServiceLifetime.")
                .Append(GetServiceLifetimeName(lifetime)).Append("));");
            sb.AppendLine();
        }

        context.AddSource(
            $"Ws2.DependencyInjection.ServiceCollectionExtensions.{marker}.g.cs",
            SourceText.From(
                //lang=cs
                $$"""
                  #nullable enable
                  namespace Ws2.DependencyInjection.Extensions
                  {
                      internal static partial class ServiceCollectionExtensions
                      {
                          private static void Add{{marker}}Services(global::Microsoft.Extensions.DependencyInjection.IServiceCollection services)
                          {
                  {{sb}}        }
                      }
                  }
                  """,
                Encoding.UTF8)
        );
    }

    private static string GetServiceLifetimeName(ServiceLifetime lifetime)
    {
        return lifetime switch
        {
            ServiceLifetime.Singleton => nameof(ServiceLifetime.Singleton),
            ServiceLifetime.Scoped => nameof(ServiceLifetime.Scoped),
            ServiceLifetime.Transient => nameof(ServiceLifetime.Transient),
            _ => throw new ArgumentOutOfRangeException(nameof(lifetime), lifetime, null)
        };
    }

    private static EquatableArray<ValueServiceDescriptor> GetDescriptors(
        GeneratorAttributeSyntaxContext context,
        ServiceLifetime lifetime
    )
    {
        if (context.TargetSymbol is not INamedTypeSymbol targetSymbol)
        {
            return EquatableArray<ValueServiceDescriptor>.Empty;
        }

        var result = new ValueServiceDescriptor[context.Attributes.Length];
        for (var i = result.Length - 1; i >= 0; i--)
        {
            var attribute = context.Attributes[i];
            object? serviceKey = null;
            string? serviceTypeName = null;

            if (attribute.AttributeClass is null)
            {
                continue;
            }

            if (attribute.AttributeClass.IsGenericType)
            {
                var serviceTypeSymbol = attribute.AttributeClass.TypeArguments[0];
                serviceTypeName = serviceTypeSymbol.ToString();
            }

            foreach (var argument in attribute.NamedArguments)
            {
                if (argument.Key == "ServiceType")
                {
                    Debug.Assert(serviceTypeName is null);
                    serviceTypeName = argument.Value.Value?.ToString();
                }
                else if (argument.Key == "ServiceKey")
                {
                    serviceKey = argument.Value.Value;
                }
            }

            result[i] = new ValueServiceDescriptor(
                targetSymbol.ToString(),
                serviceKey,
                serviceTypeName,
                lifetime
            );
        }

        return new EquatableArray<ValueServiceDescriptor>(result);
    }
}