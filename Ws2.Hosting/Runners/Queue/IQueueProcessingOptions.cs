namespace Ws2.Hosting.Runners.Queue;

public interface IQueueProcessingOptions
{
    int? ChannelCapacity { get; }

    bool ProcessSingle => true;

    int BatchSize => 100;
}