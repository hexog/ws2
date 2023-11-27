namespace Ws2.Hosting.Runners.Regular;

public interface IRegularProcessOptions
{
    bool IsEnabled { get; }
    TimeSpan Period { get; }
}