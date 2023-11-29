namespace Ws2.Hosting.StartupTasks;

public interface IStartupTask
{
    ValueTask ExecuteAsync(CancellationToken cancellationToken);

    int Priority { get; }
}