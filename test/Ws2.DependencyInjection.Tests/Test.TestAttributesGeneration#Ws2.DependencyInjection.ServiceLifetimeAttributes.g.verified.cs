//HintName: Ws2.DependencyInjection.ServiceLifetimeAttributes.g.cs
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