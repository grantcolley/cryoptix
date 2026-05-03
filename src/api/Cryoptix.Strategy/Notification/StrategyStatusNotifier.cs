using Cryoptix.Observer.Notification;

namespace Cryoptix.Strategy.Notification
{
    public sealed class StrategyStatusNotifier(
        INotificationBroadcaster notificationBroadcaster) : IStrategyStatusNotifier
    {
        private readonly INotificationBroadcaster _notificationBroadcaster = notificationBroadcaster;

        public Task NotifyUpdatedAsync(Strategies.Strategy strategy, CancellationToken cancellationToken = default)
        {
            return _notificationBroadcaster.BroadcastAsync(
                MessageType.StrategyUpdated,
                strategy,
                cancellationToken);
        }
    }
}
