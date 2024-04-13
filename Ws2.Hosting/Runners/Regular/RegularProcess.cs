using Microsoft.Extensions.Logging;

namespace Ws2.Hosting.Runners.Regular;

public interface IRegularProcessRunner
{
    Task RunAsync(CancellationToken cancellationToken);
}

public abstract class RegularProcess<TRunner> : BackgroundRunnerBase<TRunner>
    where TRunner : IRegularProcessRunner
{
    private readonly TimeProvider timeProvider;
    private long lastExecutionTimestamp;

    protected RegularProcess(ILoggerFactory loggerFactory, IServiceProvider serviceProvider, TimeProvider timeProvider)
        : base(loggerFactory, serviceProvider)
    {
        this.timeProvider = timeProvider;
    }

    protected override async Task ExecuteRunnerAsync(TRunner runner, CancellationToken cancellationToken)
    {
        lastExecutionTimestamp = timeProvider.GetTimestamp();
        await runner.RunAsync(cancellationToken).ConfigureAwait(false);
    }

    protected override Task WaitNextAsync(CancellationToken cancellationToken)
    {
        var timeSinceLastExecution = timeProvider.GetElapsedTime(lastExecutionTimestamp);
        var difference = Interval - timeSinceLastExecution;
        if (difference < TimeSpan.Zero)
        {
            return Task.CompletedTask;
        }

        return Task.Delay(difference, cancellationToken);
    }

    protected abstract TimeSpan Interval { get; }
}