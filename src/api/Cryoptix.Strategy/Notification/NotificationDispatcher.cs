using Cryoptix.Market.Models;
using Cryoptix.Observer.Notification;

namespace Cryoptix.Strategy.Notification
{
    public sealed class NotificationDispatcher(INotificationBroadcaster notificationBroadcaster) : INotificationDispatcher
    {
        private readonly INotificationBroadcaster _notificationBroadcaster = notificationBroadcaster;

        public Task PublishAsync(Kline kline, CancellationToken cancellationToken = default)
        {
            string payload = System.Text.Json.JsonSerializer.Serialize(kline);
            return _notificationBroadcaster.BroadcastAsync(
                "Kline",
                payload,
                cancellationToken);
        }

        public Task PublishAsync(Trade trade, CancellationToken cancellationToken = default)
        {
            string payload = System.Text.Json.JsonSerializer.Serialize(trade);
            return _notificationBroadcaster.BroadcastAsync(
                "Trade",
                payload,
                cancellationToken);
        }
    }
}
