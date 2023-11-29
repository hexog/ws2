using System.Threading.Channels;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Ws2.Hosting.Runners.Queue;

public abstract class QueueProcessor<TOptions, TMessage> : BackgroundService
    where TOptions : class, IQueueProcessorOptions
{
    protected readonly ILogger Logger;
    private readonly Channel<TMessage> channel;
    private readonly TOptions options;
    private readonly string processorName;

    protected QueueProcessor(ILoggerFactory loggerFactory, IOptions<TOptions> options)
    {
        Logger = loggerFactory.CreateLogger(GetType());

        this.options = options.Value;
        processorName = GetType().Name;
        if (options.Value.ChannelCapacity is { } channelCapacity)
        {
            channel = Channel.CreateBounded<TMessage>(channelCapacity);
        }
        else
        {
            channel = Channel.CreateUnbounded<TMessage>();
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Logger.LogInformation("Starting queue processor {ProcessorName}", processorName);

        try
        {
            if (options.ProcessSingle)
            {
                await ExecuteSingleAsync(stoppingToken).ConfigureAwait(false);
            }
            else
            {
                await ExecuteBatchedAsync(stoppingToken).ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException e)
        {
            Logger.LogInformation(e, "Cancelling execution of queue processor {ProcessorName}", processorName);
        }
        catch (Exception e)
        {
            Logger.LogError(e, "Unexpected error during execution of queue processor  {ProcessorName}", processorName);
        }
        finally
        {
            Logger.LogInformation("Stopped queue processor {ProcessorName}", processorName);
        }
    }

    private async Task ExecuteSingleAsync(CancellationToken stoppingToken)
    {
        var reader = channel.Reader;
        while (!stoppingToken.IsCancellationRequested)
        {
            await foreach (var message in reader.ReadAllAsync(stoppingToken).ConfigureAwait(false))
            {
                await HandleSingleAsync(message, stoppingToken).ConfigureAwait(false);
            }
        }
    }

    private async Task ExecuteBatchedAsync(CancellationToken stoppingToken)
    {
        var reader = channel.Reader;
        var batch = new List<TMessage>(options.BatchSize);
        while (!stoppingToken.IsCancellationRequested)
        {
            await reader.WaitToReadAsync(stoppingToken).ConfigureAwait(false);
            while (reader.TryRead(out var item))
            {
                batch.Add(item);
                if (batch.Count >= options.BatchSize)
                {
                    await HandleBatchAsync(batch, stoppingToken).ConfigureAwait(false);
                    batch.Clear();
                }
            }
        }
    }

    protected virtual Task HandleSingleAsync(TMessage message, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    protected virtual Task HandleBatchAsync(IReadOnlyList<TMessage> message, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    protected bool Enqueue(TMessage message)
    {
        return channel.Writer.TryWrite(message);
    }

    protected ValueTask EnqueueAsync(TMessage message, CancellationToken cancellationToken)
    {
        return channel.Writer.WriteAsync(message, cancellationToken);
    }
}