using Cryoptix.Observer.Notification;
using Cryoptix.Strategy.Cache;

namespace Cryoptix.Strategy.Notification
{
    /// <summary>
    /// Represents the strategy status notifier.
    /// </summary>
    public sealed class StrategyStatusNotifier(
        INotificationBroadcaster notificationBroadcaster,
        IMarketDataSnapshotProvider marketDataSnapshotProvider) : IStrategyStatusNotifier
    {
        /// <summary>
        /// Executes the notify started async operation.
        /// </summary>
        /// <param name="strategy">The strategy value.</param>
        /// <param name="cancellationToken">The cancellation token value.</param>
        /// <returns>The notify started async result.</returns>
        public Task NotifyStartedAsync(Strategies.Strategy strategy, CancellationToken cancellationToken = default)
        {
            return notificationBroadcaster.BroadcastAsync(
                MessageType.StrategyStarted,
                strategy,
                cancellationToken);
        }

        /// <summary>
        /// Executes the notify market data snapshot async operation.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token value.</param>
        /// <returns>The notify market data snapshot async result.</returns>
        public async Task NotifyMarketDataSnapshotAsync(CancellationToken cancellationToken = default)
        {
            MarketDataSnapshot marketDataSnapshot = await marketDataSnapshotProvider.GetSnapshotAsync(cancellationToken);

            await notificationBroadcaster.BroadcastAsync(
                MessageType.MarketDataSnapshot,
                marketDataSnapshot,
                cancellationToken);
        }

        /// <summary>
        /// Executes the notify updated async operation.
        /// </summary>
        /// <param name="strategy">The strategy value.</param>
        /// <param name="cancellationToken">The cancellation token value.</param>
        /// <returns>The notify updated async result.</returns>
        public Task NotifyUpdatedAsync(Strategies.Strategy strategy, CancellationToken cancellationToken = default)
        {
            return notificationBroadcaster.BroadcastAsync(
                MessageType.StrategyUpdated,
                strategy,
                cancellationToken);
        }
    }
}
