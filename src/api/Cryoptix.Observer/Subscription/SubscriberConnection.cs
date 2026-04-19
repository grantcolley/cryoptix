namespace Cryoptix.Observer.Subscription
{
    public sealed class SubscriberConnection
    {
        public string ConnectionId { get; init; } = default!;
        public string UserId { get; init; } = default!;
        public string? TenantId { get; init; }
        public DateTime ConnectedAtUtc { get; init; }
    }
}
