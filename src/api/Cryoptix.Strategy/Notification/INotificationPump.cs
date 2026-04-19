using Cryoptix.Strategy.Channel;

namespace Cryoptix.Strategy.Notification
{
    public interface INotificationPump
    {
        Task RunAsync(StrategyEventChannels channels, CancellationToken cancellationToken);
    }
}
