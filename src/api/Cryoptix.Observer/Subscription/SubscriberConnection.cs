namespace Cryoptix.Observer.Subscription
{
    /// <summary>
    /// Represents the subscriber connection.
    /// </summary>
    public sealed class SubscriberConnection
    {
        /// <summary>
        /// Gets or sets the connection id.
        /// </summary>
        public string ConnectionId { get; init; } = default!;
        /// <summary>
        /// Gets or sets the user id.
        /// </summary>
        public string UserId { get; init; } = default!;
        /// <summary>
        /// Gets or sets the tenant id.
        /// </summary>
        public string? TenantId { get; init; }
        /// <summary>
        /// Gets or sets the connected at utc.
        /// </summary>
        public DateTime ConnectedAtUtc { get; init; }
    }
}
