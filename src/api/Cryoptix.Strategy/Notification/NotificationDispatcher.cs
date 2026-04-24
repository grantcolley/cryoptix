using Cryoptix.Market.Data;
using Cryoptix.Observer.Notification;

namespace Cryoptix.Strategy.Notification
{
    public sealed class NotificationDispatcher(
        INotificationBroadcaster notificationBroadcaster) : INotificationDispatcher
    {
        private readonly INotificationBroadcaster _notificationBroadcaster = notificationBroadcaster;

        public Task PublishAsync(Kline kline, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(kline);

            return _notificationBroadcaster.BroadcastAsync(
                MessageType.Kline,
                kline,
                cancellationToken);
        }

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
