namespace Ws2.Hosting.Runners.Queue;

public interface IQueueProcessorOptions
{
    int? ChannelCapacity { get; }

    bool ProcessSingle => true;

    int BatchSize => 100;
}