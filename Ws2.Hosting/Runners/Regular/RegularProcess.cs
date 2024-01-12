using System.Diagnostics;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Ws2.Hosting.Runners.Regular;

public abstract class RegularProcess<TOptions> : BackgroundService
    where TOptions : IRegularProcessOptions
{
    private readonly RegularProcessState state = new();
    protected readonly ILogger Logger;
    protected readonly IOptionsMonitor<TOptions> OptionsMonitor;
    private readonly string processName;

    protected RegularProcess(ILoggerFactory loggerFactory, IOptionsMonitor<TOptions> optionsMonitor)
    {
        Logger = loggerFactory.CreateLogger(GetType());
        OptionsMonitor = optionsMonitor;
        processName = GetType().Name;
        state.Initialize(OptionsMonitor.CurrentValue);
        optionsMonitor.OnChange(state.HandleUpdateOptions);
    }

    protected abstract Task RunAsync(CancellationToken stoppingToken);

    private async Task InnerExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var options = OptionsMonitor.CurrentValue;
            if (!options.IsEnabled)
            {
                var pauseTask = state.PauseTask;
                Logger.LogInformation("Paused regular process {ProcessName}", processName);
                await pauseTask.ConfigureAwait(false);
                Logger.LogInformation("Unpaused regular process: {ProcessName}", processName);
                continue;
            }

            try
            {
                if (options.Period < TimeSpan.FromMinutes(1))
                {
                    await RunAsync(stoppingToken).ConfigureAwait(false);
                }
                else
                {
                    Logger.LogInformation("Executing regular process {ProcessName}", processName);
                    await RunAsync(stoppingToken).ConfigureAwait(false);
                    Logger.LogInformation("Completed execution of regular process {ProcessName}", processName);
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
                    "Unexpected error during execution of regular process {RegularProcess}",
                    processName
                );
            }

            try
            {
                await Task.Delay(options.Period, state.RestartToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                Logger.LogInformation("Resetting regular process {ProcessName}", processName);
            }
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Logger.LogInformation("Starting regular process {ProcessName}", processName);

        state.Enable(stoppingToken);

        try
        {
            await InnerExecuteAsync(stoppingToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException e)
        {
            Logger.LogWarning(e, "Canceled execution of regular process {ProcessName}", processName);
        }
        finally
        {
            Logger.LogInformation("Stopped regular process {ProcessName}", processName);
        }
    }

    #region Regular Process State

    private class RegularProcessState
    {
        private CancellationToken? processStoppingToken;
        private CancellationTokenSource? restartTokenSource;
        private TaskCompletionSource? pauseTaskSource;
        private TOptions? cachedOptions;
        private readonly object lockObject = new();

        public void Initialize(TOptions options)
        {
            cachedOptions = options;
            if (!options.IsEnabled)
            {
                pauseTaskSource = new TaskCompletionSource();
            }
        }

        public void Enable(CancellationToken stoppingToken)
        {
            lock (lockObject)
            {
                Debug.Assert(processStoppingToken is null);
                processStoppingToken = stoppingToken;
                Debug.Assert(restartTokenSource is null);
                restartTokenSource = CancellationTokenSource.CreateLinkedTokenSource(processStoppingToken.Value);
            }

            stoppingToken.Register(Disable);
        }

        public void Pause()
        {
            lock (lockObject)
            {
                Restart();
                Debug.Assert(pauseTaskSource is not null);
                pauseTaskSource.SetResult();
                pauseTaskSource = null;
            }
        }

        public void Resume()
        {
            lock (lockObject)
            {
                Debug.Assert(pauseTaskSource is null);
                pauseTaskSource = new TaskCompletionSource();
                Restart();
            }
        }

        public void Restart()
        {
            lock (lockObject)
            {
                Debug.Assert(restartTokenSource is not null);
                restartTokenSource.Cancel();
                Debug.Assert(processStoppingToken is not null);
                restartTokenSource = CancellationTokenSource.CreateLinkedTokenSource(processStoppingToken.Value);
            }
        }

        public void Disable()
        {
            lock (lockObject)
            {
                Debug.Assert(processStoppingToken is not null);
                pauseTaskSource?.SetCanceled(processStoppingToken.Value);
            }
        }

        public void HandleUpdateOptions(TOptions newOptions)
        {
            lock (lockObject)
            {
                Debug.Assert(cachedOptions is not null);
                if (cachedOptions.IsEnabled != newOptions.IsEnabled)
                {
                    if (newOptions.IsEnabled)
                    {
                        Pause();
                    }
                    else
                    {
                        Resume();
                    }
                }

                if (cachedOptions.Period != newOptions.Period)
                {
                    Restart();
                }

                cachedOptions = newOptions;
            }
        }


        public Task PauseTask
        {
            get
            {
                lock (lockObject)
                {
                    Debug.Assert(pauseTaskSource is not null);
                    return pauseTaskSource.Task;
                }
            }
        }

        public CancellationToken RestartToken
        {
            get
            {
                lock (lockObject)
                {
                    Debug.Assert(restartTokenSource is not null);
                    return restartTokenSource.Token;
                }
            }
        }
    }

    #endregion
}