using Cryoptix.Observer.Notification;
using Cryoptix.Observer.Subscription;
using Microsoft.AspNetCore.SignalR;

namespace Cryoptix.Web.API.Notification
{
    internal sealed class SignalRNotificationBroadcaster(
        ISubscriptionManager subscriptionManager,
        IHubContext<NotificationHub> hubContext,
        ILogger<SignalRNotificationBroadcaster> logger) : INotificationBroadcaster
    {
        private readonly ISubscriptionManager _subscriptionManager = subscriptionManager;
        private readonly IHubContext<NotificationHub> _hubContext = hubContext;
        private readonly ILogger<SignalRNotificationBroadcaster> _logger = logger;

        public async Task BroadcastAsync<TPayload>(
            MessageType messageType,
            TPayload payload,
            CancellationToken cancellationToken = default)
        {
            if (messageType == MessageType.None)
                throw new ArgumentException("MessageType.None is not valid for notifications.", nameof(messageType));

            ArgumentNullException.ThrowIfNull(payload);

            NotificationEnvelope envelope = new()
            {
                MessageType = messageType,
                TimestampUtc = DateTime.UtcNow,
                Payload = payload
            };

            IReadOnlyCollection<SubscriberConnection> subscribers =
                await _subscriptionManager.GetAllAsync(cancellationToken);

            if (subscribers.Count == 0)
            {
                return;
            }

            string[] connectionIds = [.. subscribers
                .Select(x => x.ConnectionId)
                .Where(x => !string.IsNullOrWhiteSpace(x))];

            if (connectionIds.Length == 0)
            {
                return;
            }

            try
            {
                await _hubContext.Clients
                    .Clients(connectionIds)
                    .SendAsync("ReceiveNotification", envelope, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed broadcasting notification. MessageType={MessageType}",
                    envelope.MessageType);

                throw;
            }
        }
    }
}
