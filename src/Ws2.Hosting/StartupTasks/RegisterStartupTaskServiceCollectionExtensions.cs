using Microsoft.Extensions.DependencyInjection.Extensions;
using Ws2.Hosting.StartupTasks;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class RegisterStartupTaskServiceCollectionExtensions
{
    public static IServiceCollection AddStartupTask(this IServiceCollection serviceCollection, IStartupTask startupTask)
    {
        serviceCollection.TryAddEnumerable(ServiceDescriptor.Singleton(startupTask));
        return serviceCollection;
    }

    public static IServiceCollection AddStartupTask(this IServiceCollection serviceCollection, Type startupTaskType)
    {
        serviceCollection.TryAddEnumerable(ServiceDescriptor.Transient(typeof(IStartupTask), startupTaskType));
        return serviceCollection;
    }

    public static IServiceCollection AddStartupTask<TStartupTask>(this IServiceCollection serviceCollection)
        where TStartupTask : class, IStartupTask
    {
        serviceCollection.TryAddEnumerable(ServiceDescriptor.Transient<IStartupTask, TStartupTask>());
        return serviceCollection;
    }

    public static IServiceCollection AddStartupTask<TStartupTask>(
        this IServiceCollection serviceCollection,
        Func<IServiceProvider, TStartupTask> factory
    )
        where TStartupTask : class, IStartupTask
    {
        serviceCollection.TryAddEnumerable(ServiceDescriptor.Transient<IStartupTask, TStartupTask>(factory));
        return serviceCollection;
    }

    public static IServiceCollection AddStartupTask(
        this IServiceCollection serviceCollection,
        Func<IServiceProvider, CancellationToken, ValueTask> runner,
        int priority
    )
    {
        serviceCollection.TryAddEnumerable(
            ServiceDescriptor.Transient<IStartupTask>(p => new ActionStartupTask(priority, runner, p)));
        return serviceCollection;
    }
}