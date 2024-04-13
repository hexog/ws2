namespace Ws2.Hosting.Runners.Queue;

public interface IQueueProcessorOptions
{
    int? ChannelCapacity { get; }
}