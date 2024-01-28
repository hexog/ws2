namespace Ws2.Hosting.StartupTasks;

public static class StartupTaskExecutor
{
    public static async Task ExecuteStartupTasksAsync(IEnumerable<IStartupTask> tasks, CancellationToken cancellationToken = default)
    {
        foreach (var taskBatch in tasks.GroupBy(x => x.Priority).OrderBy(x => x.Key).Select(x => x))
        {
            if (taskBatch.Count() == 1)
            {
                await taskBatch.First().ExecuteAsync(cancellationToken).ConfigureAwait(false);
            }
            else
            {
                await Parallel.ForEachAsync(
                    taskBatch,
                    cancellationToken,
                    static (task, token) => task.ExecuteAsync(token)
                ).ConfigureAwait(false);
            }
        }
    }
}