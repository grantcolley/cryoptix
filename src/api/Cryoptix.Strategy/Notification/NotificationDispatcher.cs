using Cryoptix.Market.Data;
using Cryoptix.Observer.Notification;

namespace Cryoptix.Strategy.Notification
{
    public sealed class NotificationDispatcher(
        INotificationBroadcaster notificationBroadcaster) : INotificationDispatcher
    {
        private readonly INotificationBroadcaster _notificationBroadcaster = notificationBroadcaster;

        /// <summary>
        /// Publishes a kline notification using the configured broadcaster.
        /// </summary>
        /// <param name="kline">The kline data to publish.</param>
        /// <param name="cancellationToken">Cancellation token for the publish operation.</param>
        /// <returns>A task that completes when the broadcast operation finishes.</returns>
        public Task PublishAsync(Kline kline, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(kline);

            return _notificationBroadcaster.BroadcastAsync(
                MessageType.Kline,
                kline,
                cancellationToken);
        }

        /// <summary>
        /// Publishes a trade notification using the configured broadcaster.
        /// </summary>
        /// <param name="trade">The trade data to publish.</param>
        /// <param name="cancellationToken">Cancellation token for the publish operation.</param>
        /// <returns>A task that completes when the broadcast operation finishes.</returns>
        public Task PublishAsync(Trade trade, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(trade);

            return _notificationBroadcaster.BroadcastAsync(
                MessageType.Trade,
                trade,
                cancellationToken);
        }
    }
}
