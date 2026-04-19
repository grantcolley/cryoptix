using Cryoptix.Observer.Notification;
using Cryoptix.Observer.Subscription;
using Microsoft.AspNetCore.SignalR;

namespace Cryoptix.Web.API.Notification
{
    public sealed class SignalRNotificationBroadcaster : INotificationBroadcaster
    {
        private const string ClientMethodName = "ReceiveNotification";

        private readonly ISubscriptionManager _subscriptionManager;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly ILogger<SignalRNotificationBroadcaster> _logger;

        public SignalRNotificationBroadcaster(
            ISubscriptionManager subscriptionManager,
            IHubContext<NotificationHub> hubContext,
            ILogger<SignalRNotificationBroadcaster> logger)
        {
            _subscriptionManager = subscriptionManager;
            _hubContext = hubContext;
            _logger = logger;
        }

        public async Task BroadcastAsync(
            string messageType,
            string payloadJson,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(messageType))
            {
                throw new ArgumentException("Message type is required.", nameof(messageType));
            }

            if (string.IsNullOrWhiteSpace(payloadJson))
            {
                throw new ArgumentException("Payload JSON is required.", nameof(payloadJson));
            }

            var envelope = new NotificationEnvelope
            {
                MessageType = messageType,
                TimestampUtc = DateTime.UtcNow,
                Payload = payloadJson
            };

            var subscribers = await _subscriptionManager.GetAllAsync(cancellationToken);

            if (subscribers.Count == 0)
            {
                _logger.LogDebug("No subscribers registered. Notification skipped.");
                return;
            }

            var connectionIds = subscribers
                .Select(x => x.ConnectionId)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToArray();

            try
            {
                await _hubContext.Clients
                    .Clients(connectionIds)
                    .SendAsync(ClientMethodName, envelope, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed broadcasting notification. MessageType={MessageType}. Payload={payloadJson}",
                    envelope.MessageType,
                    payloadJson);

                throw;
            }
        }
    }
}
