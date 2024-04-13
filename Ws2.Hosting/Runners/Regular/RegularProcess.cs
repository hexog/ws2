using System.Diagnostics;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Ws2.Hosting.Runners.Regular;

public abstract class RegularProcess : BackgroundService
{
    protected readonly ILogger Logger;
    private readonly string regularProcessName;

    private ulong runId = 0;
    private DateTime lastExecutionTimestamp = DateTime.MinValue;
    private readonly object lastExecutionTimestampLock = new();

    private const int RetryAttemptCount = 2;

    protected RegularProcess(ILoggerFactory loggerFactory)
    {
        var type = GetType();
        Logger = loggerFactory.CreateLogger(type);
        regularProcessName = type.Name;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Logger.LogInformation("Starting regular process {RegularProcessName}", regularProcessName);
        try
        {
            while (true)
            {
                var executionTime = TimeSpan.Zero;
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

                    for (var i = RetryAttemptCount; i >= 0; i--)
                    {
                        try
                        {
                            executionTime = await CompleteNextRunAsync(stoppingToken).ConfigureAwait(false);
                            break;
                        }
                        catch (OperationCanceledException)
                        {
                            throw;
                        }
                        catch when (i == 0)
                        {
                            throw;
                        }
                        catch (Exception e)
                        {
                            Logger.LogWarning(
                                e,
                                "Unexpected error during regular process {RegularProcessName} execution, retries left: {RetriesLeft}",
                                regularProcessName,
                                i
                            );
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    Logger.LogError(
                        e,
                        "Unexpected error during regular process {RegularProcessName} execution",
                        regularProcessName
                    );
                }

                await Task.Delay(Interval - executionTime, stoppingToken).ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException)
        {
            Logger.LogWarning("Regular process {RegularProcessName} was canceled", regularProcessName);
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