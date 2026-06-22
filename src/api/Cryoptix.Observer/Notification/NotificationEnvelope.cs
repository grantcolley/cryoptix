namespace Cryoptix.Observer.Notification
{
    /// <summary>
    /// Represents the notification envelope.
    /// </summary>
    public sealed class NotificationEnvelope
    {
        /// <summary>
        /// Gets or sets the message type.
        /// </summary>
        public MessageType MessageType { get; init; }
        /// <summary>
        /// Gets or sets the timestamp utc.
        /// </summary>
        public DateTime TimestampUtc { get; init; }
        /// <summary>
        /// Gets or sets the payload.
        /// </summary>
        public object? Payload { get; init; }
    }
}
