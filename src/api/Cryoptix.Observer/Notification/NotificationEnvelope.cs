namespace Cryoptix.Observer.Notification
{
    public sealed class NotificationEnvelope
    {
        public string MessageType { get; init; } = default!;
        public DateTime TimestampUtc { get; init; }
        public string Payload { get; init; } = default!;
    }
}
