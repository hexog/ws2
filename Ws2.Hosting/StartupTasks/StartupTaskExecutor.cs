namespace Ws2.Hosting.StartupTasks;

public class StartupTaskExecutor
{
    public static async Task ExecuteStartupTasksAsync(IEnumerable<IStartupTask> tasks, CancellationToken cancellationToken = default)
    {
        foreach (var taskBatch in tasks.GroupBy(x => x.Priority).OrderBy(x => x.Key).Select(x => x))
        {
            await Parallel.ForEachAsync(
                taskBatch,
                cancellationToken,
                (task, token) => task.ExecuteAsync(token)
            ).ConfigureAwait(false);
        }
    }
}