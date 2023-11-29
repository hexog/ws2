﻿namespace Ws2.DependencyInjection.Abstractions;

public interface IServiceTypeImplementationRegistrar
{
    Type ServiceType { get; }

    void Register(ServiceAttributeRegistrarContext context, Type type);
}

public interface IServiceTypeImplementationRegistrar<TServiceType> : IServiceTypeImplementationRegistrar
    where TServiceType : class
{
    Type IServiceTypeImplementationRegistrar.ServiceType => typeof(TServiceType);
}