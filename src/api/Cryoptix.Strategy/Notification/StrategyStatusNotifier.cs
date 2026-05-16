using Cryoptix.Observer.Notification;
using Cryoptix.Strategy.Cache;

namespace Cryoptix.Strategy.Notification
{
    public sealed class StrategyStatusNotifier(
        INotificationBroadcaster notificationBroadcaster,
        IMarketDataSnapshotProvider marketDataSnapshotProvider) : IStrategyStatusNotifier
    {
        public Task NotifyStartedAsync(Strategies.Strategy strategy, CancellationToken cancellationToken = default)
        {
            return notificationBroadcaster.BroadcastAsync(
                MessageType.StrategyStarted,
                strategy,
                cancellationToken);
        }

        public async Task NotifyMarketDataSnapshotAsync(CancellationToken cancellationToken = default)
        {
            MarketDataSnapshot marketDataSnapshot = await marketDataSnapshotProvider.GetSnapshotAsync(cancellationToken);

            await notificationBroadcaster.BroadcastAsync(
                MessageType.MarketDataSnapshot,
                marketDataSnapshot,
                cancellationToken);
        }

        public Task NotifyUpdatedAsync(Strategies.Strategy strategy, CancellationToken cancellationToken = default)
        {
            return notificationBroadcaster.BroadcastAsync(
                MessageType.StrategyUpdated,
                strategy,
                cancellationToken);
        }
    }
}
