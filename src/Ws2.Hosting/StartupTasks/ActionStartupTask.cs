namespace Ws2.Hosting.StartupTasks;

public class ActionStartupTask(int priority, Func<IServiceProvider, CancellationToken, ValueTask> runner, IServiceProvider services)
    : IStartupTask
{
    public async ValueTask ExecuteAsync(CancellationToken cancellationToken)
    {
        await runner(services, cancellationToken).ConfigureAwait(false);
    }

    public int Priority => priority;
}