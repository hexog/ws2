using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Ws2.Hosting.Runners;

public abstract class BackgroundRunnerBase<TRunner> : BackgroundService
    where TRunner : notnull
{
    private readonly IServiceProvider serviceProvider;
    private ulong runId;

    private const int MaxRetryCount = 2;

    protected BackgroundRunnerBase(ILoggerFactory loggerFactory, IServiceProvider serviceProvider)
    {
        Logger = loggerFactory.CreateLogger(GetType());
        this.serviceProvider = serviceProvider;
    }

    protected ILogger Logger { get; }

    protected abstract Task ExecuteRunnerAsync(TRunner runner, CancellationToken cancellationToken);

    protected abstract Task WaitNextAsync(CancellationToken cancellationToken);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Logger.LogInformation("Started runner");
        try
        {
            while (true)
            {
                using (_ = Logger.BeginScope("RequestId: {RequestId}", runId.ToString()))
                await using (var scope = serviceProvider.CreateAsyncScope())
                {
                    var retryCount = MaxRetryCount;
                    do
                    {
                        try
                        {
                            await ExecuteRunnerAsync(
                                scope.ServiceProvider.GetRequiredService<TRunner>(),
                                stoppingToken
                            ).ConfigureAwait(false);
                            break;
                        }
                        catch (OperationCanceledException)
                        {
                            throw;
                        }
                        catch (Exception e)
                        {
                            Logger.LogError(e, "Error during runner execution, retries left: {RetriesLeft}", retryCount);
                            retryCount--;
                            await Task.Delay(500, stoppingToken).ConfigureAwait(false);
                        }
                    } while (retryCount >= 0);

                    runId++;
                }

                await WaitNextAsync(stoppingToken).ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException)
        {
            // ignored
        }
        catch (Exception e)
        {
            Logger.LogError(e, "Unexpected error during runner execution");
        }

        Logger.LogInformation("Stopped runner");
    }
}