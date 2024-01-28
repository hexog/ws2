using System.Diagnostics;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Ws2.Hosting.Runners.Regular;

public abstract class RegularProcess : BackgroundService
{
    protected readonly ILogger Logger;
    private readonly string regularProcessName;

    private ulong runId = 0;
    private DateTime lastExecutionTimestamp = DateTime.MinValue;
    private readonly object lastExecutionTimestampLock = new();

    protected RegularProcess(ILoggerFactory loggerFactory)
    {
        var type = GetType();
        Logger = loggerFactory.CreateLogger(type);
        regularProcessName = type.Name;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Logger.LogInformation("Starting regular process {RegularProcessName}", regularProcessName);
        while (true)
        {
            try
            {
                var now = DateTime.UtcNow;
                TimeSpan timeSinceLastExecution;
                lock (lastExecutionTimestampLock)
                {
                    timeSinceLastExecution = now - lastExecutionTimestamp;
                }

                if (timeSinceLastExecution < Interval)
                {
                    await Task.Delay(Interval - timeSinceLastExecution, stoppingToken).ConfigureAwait(false);
                    continue;
                }

                var executionTime = await CompleteNextRunAsync(stoppingToken).ConfigureAwait(false);
                await Task.Delay(Interval - executionTime, stoppingToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                Logger.LogWarning("Regular process {RegularProcessName} was canceled", regularProcessName);
                break;
            }
            catch (Exception e)
            {
                Logger.LogError(
                    e,
                    "Unexpected error during regular process {RegularProcessName} execution",
                    regularProcessName
                );
            }
        }

        Logger.LogInformation("Stopped regular process {RegularProcessName}", regularProcessName);
    }

    public async ValueTask<TimeSpan> CompleteNextRunAsync(CancellationToken cancellationToken)
    {
        TimeSpan executionTime;
        using (Logger.BeginScope("RequestId: {RequestId}", runId.ToString()))
        {
            var startedTimestamp = Stopwatch.GetTimestamp();
            await RunAsync(cancellationToken).ConfigureAwait(false);
            executionTime = Stopwatch.GetElapsedTime(startedTimestamp);
            Logger.LogInformation(
                "Completed execution of regular process {RegularProcessName} in {ElapsedSeconds} seconds",
                regularProcessName,
                executionTime.TotalSeconds
            );
        }

        lock (lastExecutionTimestampLock)
        {
            lastExecutionTimestamp = DateTime.UtcNow;
        }

        runId++;
        return executionTime;
    }

    protected abstract Task RunAsync(CancellationToken cancellationToken);

    protected abstract TimeSpan Interval { get; }
}