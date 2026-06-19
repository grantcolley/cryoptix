using Cryoptix.Observer.Authorization;
using Cryoptix.Observer.Notification;
using Cryoptix.Observer.Subscription;
using Cryoptix.Strategy.Cache;
using Cryoptix.Web.API.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Cryoptix.Web.API.Notification
{
    [Authorize(Policy = Claims.CRYOPTIX_USER_CLAIM)]
    public sealed class NotificationHub(
        ISubscriptionManager subscriptionManager,
        IUserContextAccessor userContextAccessor,
        IMarketDataSnapshotProvider marketDataSnapshotProvider,
        ILogger<NotificationHub> logger) : Hub
    {
        public override async Task OnConnectedAsync()
        {
            var user = Context.User
                ?? throw new InvalidOperationException("Hub connection has no authenticated user.");

            MarketDataSnapshot snapshot =
                await marketDataSnapshotProvider.GetSnapshotAsync(Context.ConnectionAborted);

            NotificationEnvelope envelope = new()
            {
                MessageType = MessageType.MarketDataSnapshot,
                TimestampUtc = DateTime.UtcNow,
                Payload = snapshot
            };

            await Clients.Caller.SendAsync(
                "ReceiveNotification",
                envelope,
                Context.ConnectionAborted);

            var subscriber = new SubscriberConnection
            {
                ConnectionId = Context.ConnectionId,
                UserId = userContextAccessor.GetUserId(user),
                TenantId = userContextAccessor.GetTenantId(user),
                ConnectedAtUtc = DateTime.UtcNow
            };

            await subscriptionManager.RegisterAsync(subscriber, Context.ConnectionAborted);

            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation(
                "Registered subscriber after market data snapshot. ConnectionId={ConnectionId}.",
                subscriber.ConnectionId);
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await subscriptionManager.UnregisterAsync(Context.ConnectionId);

            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation(
                    "Unregistered subscriber. ConnectionId={ConnectionId}",
                    Context.ConnectionId);
            }

            await base.OnDisconnectedAsync(exception);
        }
    }
}
