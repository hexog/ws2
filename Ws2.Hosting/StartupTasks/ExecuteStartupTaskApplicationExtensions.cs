using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Ws2.Hosting.StartupTasks;

public static class ExecuteStartupTaskApplicationExtensions
{
    public static void ExecuteStartupTasks(this IHost host)
    {
        ExecuteStartupTasks(host.Services);
    }

    public static Task ExecuteStartupTasksAsync(this IHost host, CancellationToken cancellationToken = default)
    {
        return ExecuteStartupTasksAsync(host.Services, cancellationToken);
    }

    public static void ExecuteStartupTasks(this IServiceProvider serviceProvider)
    {
        ExecuteStartupTasksAsync(serviceProvider).GetAwaiter().GetResult();
    }

    public static async Task ExecuteStartupTasksAsync(
        this IServiceProvider serviceProvider,
        CancellationToken cancellationToken = default
    )
    {
        await using var serviceScope = serviceProvider.CreateAsyncScope();
        var startupTasks = serviceProvider.GetServices<IStartupTask>();
        await StartupTaskExecutor.ExecuteStartupTasksAsync(startupTasks, cancellationToken).ConfigureAwait(false);
    }
}