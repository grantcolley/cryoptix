using Cryoptix.Observer.Authorization;
using Cryoptix.Observer.Subscription;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Cryoptix.Web.API.Notification
{
    [Authorize]
    public sealed class NotificationHub : Hub
    {
        private readonly ISubscriptionManager _subscriptionManager;
        private readonly IUserContextAccessor _userContextAccessor;
        private readonly ILogger<NotificationHub> _logger;

        public NotificationHub(
            ISubscriptionManager subscriptionManager,
            IUserContextAccessor userContextAccessor,
            ILogger<NotificationHub> logger)
        {
            _subscriptionManager = subscriptionManager;
            _userContextAccessor = userContextAccessor;
            _logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            var user = Context.User
                ?? throw new InvalidOperationException("Hub connection has no authenticated user.");

            var subscriber = new SubscriberConnection
            {
                ConnectionId = Context.ConnectionId,
                UserId = _userContextAccessor.GetUserId(user),
                TenantId = _userContextAccessor.GetTenantId(user),
                ConnectedAtUtc = DateTime.UtcNow
            };

            await _subscriptionManager.RegisterAsync(subscriber, Context.ConnectionAborted);

            _logger.LogInformation(
                "Registered subscriber. ConnectionId={ConnectionId}, UserId={UserId}, TenantId={TenantId}",
                subscriber.ConnectionId,
                subscriber.UserId,
                subscriber.TenantId);

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await _subscriptionManager.UnregisterAsync(Context.ConnectionId);

            _logger.LogInformation(
                "Unregistered subscriber. ConnectionId={ConnectionId}",
                Context.ConnectionId);

            await base.OnDisconnectedAsync(exception);
        }
    }
}
