using System.Threading.Channels;
using Microsoft.Extensions.Logging;

namespace Ws2.Hosting.Runners.Queue;

public interface IQueueRunner<in TMessage>
{
    Task RunAsync(IReadOnlyCollection<TMessage> messages, CancellationToken cancellationToken);
}

public class QueueProcessor<TRunner, TMessage> : BackgroundRunnerBase<TRunner>
    where TRunner : IQueueRunner<TMessage>
{
    private readonly Channel<TMessage> channel = Channel.CreateUnbounded<TMessage>(
        new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = false,
        }
    );

    private readonly List<TMessage> buffer = new();

    public QueueProcessor(ILoggerFactory loggerFactory, IServiceProvider serviceProvider) : base(loggerFactory, serviceProvider)
    {
    }

    protected override async Task ExecuteRunnerAsync(TRunner runner, CancellationToken cancellationToken)
    {
        while (channel.Reader.TryRead(out var message))
        {
            buffer.Add(message);
        }

        try
        {
            await runner.RunAsync(buffer, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            buffer.Clear();
        }
    }

    protected override async Task WaitNextAsync(CancellationToken cancellationToken)
    {
        var reader = channel.Reader;
        await reader.WaitToReadAsync(cancellationToken).ConfigureAwait(false);
        if (reader is { CanCount: true, Count: < 100 })
        {
            await Task.Delay(1000, cancellationToken).ConfigureAwait(false);
        }
    }

    public bool Enqueue(TMessage message)
    {
        return channel.Writer.TryWrite(message);
    }

    public ValueTask EnqueueAsync(TMessage message, CancellationToken cancellationToken)
    {
        return channel.Writer.WriteAsync(message, cancellationToken);
    }
}