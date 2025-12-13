//HintName: Ws2.DependencyInjection.ServiceLifetimeAttributes.g.cs
#nullable enable
namespace Ws2.DependencyInjection
{
    /// <summary>
    /// </summary>
    /// <summary>
    /// Marks a class as a singleton service.
    /// </summary>
    [global::System.AttributeUsage(global::System.AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    [global::System.Diagnostics.Conditional("WS2_DI_USAGES")]
    internal sealed class SingletonAttribute : global::System.Attribute
    {
        /// <summary>
        /// Specifies the service type under which the implementation is registered.
        /// </summary>
        /// <remarks>When not specified, the implementation type is used as the service type.</remarks>
        public global::System.Type? ServiceType { get; set; }
    
        /// <summary>
        /// An optional service key used to distinguish multiple registrations of the same service type.
        /// </summary>
        public object? ServiceKey { get; set; }
    
        /// <summary>
        /// Controls whether a singleton implementation is shared across multiple service types.
        /// </summary>
        /// <remarks>
        /// When <c>true</c>, all service types mapped to the same implementation resolve
        /// to the same singleton instance. When <c>false</c>, a separate singleton instance
        /// is created for each service type.
        /// </remarks>
        public bool Shared { get; set; } = true;
    }
    
    /// <summary>
    /// Marks a type as a singleton service implementation for the specified service type.
    /// </summary>
    /// <typeparam name="TService">The service type under which the implementation is registered.</typeparam>
    [global::System.AttributeUsage(global::System.AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    [global::System.Diagnostics.Conditional("WS2_DI_USAGES")]
    internal sealed class SingletonAttribute<TService> : global::System.Attribute
    {
        /// <summary>
        /// An optional service key used to distinguish multiple registrations of the same service type.
        /// </summary>
        public object? ServiceKey { get; set; }
    
        /// <summary>
        /// Controls whether a singleton implementation is shared across multiple service types.
        /// </summary>
        /// <remarks>
        /// When <c>true</c>, all service types mapped to the same implementation resolve
        /// to the same singleton instance. When <c>false</c>, a separate singleton instance
        /// is created for each service type.
        /// </remarks>
        public bool Shared { get; set; } = true;
    }
    
    /// <summary>
    /// Marks a class as a scoped service.
    /// </summary>
    [global::System.AttributeUsage(global::System.AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    [global::System.Diagnostics.Conditional("WS2_DI_USAGES")]
    internal sealed class ScopedAttribute : global::System.Attribute
    {
        /// <summary>
        /// Specifies the service type under which the implementation is registered.
        /// </summary>
        /// <remarks>When not specified, the implementation type is used as the service type.</remarks>
        public global::System.Type? ServiceType { get; set; }
    
        /// <summary>
        /// An optional service key used to distinguish multiple registrations of the same service type.
        /// </summary>
        public object? ServiceKey { get; set; }
    }
    
    /// <summary>
    /// Marks a type as a scoped service implementation for the specified service type.
    /// </summary>
    /// <typeparam name="TService">The service type under which the implementation is registered.</typeparam>
    [global::System.AttributeUsage(global::System.AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    [global::System.Diagnostics.Conditional("WS2_DI_USAGES")]
    internal sealed class ScopedAttribute<TService> : global::System.Attribute
    {
        /// <summary>
        /// An optional service key used to distinguish multiple registrations of the same service type.
        /// </summary>
        public object? ServiceKey { get; set; }
    }
    
    /// <summary>
    /// Marks a class as a transient service.
    /// </summary>
    [global::System.AttributeUsage(global::System.AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    [global::System.Diagnostics.Conditional("WS2_DI_USAGES")]
    internal sealed class TransientAttribute : global::System.Attribute
    {
        /// <summary>
        /// Specifies the service type under which the implementation is registered.
        /// </summary>
        /// <remarks>When not specified, the implementation type is used as the service type.</remarks>
        public global::System.Type? ServiceType { get; set; }
    
        /// <summary>
        /// An optional service key used to distinguish multiple registrations of the same service type.
        /// </summary>
        public object? ServiceKey { get; set; }
    }
    
    /// <summary>
    /// Marks a type as a transient service implementation for the specified service type.
    /// </summary>
    /// <typeparam name="TService">The service type under which the implementation is registered.</typeparam>
    [global::System.AttributeUsage(global::System.AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    [global::System.Diagnostics.Conditional("WS2_DI_USAGES")]
    internal sealed class TransientAttribute<TService> : global::System.Attribute
    {
        /// <summary>
        /// An optional service key used to distinguish multiple registrations of the same service type.
        /// </summary>
        public object? ServiceKey { get; set; }
    }
}