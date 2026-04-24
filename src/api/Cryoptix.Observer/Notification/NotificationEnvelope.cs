namespace Cryoptix.Observer.Notification
{
    public sealed class NotificationEnvelope
    {
        public MessageType MessageType { get; init; }
        public DateTime TimestampUtc { get; init; }
        public object? Payload { get; init; }
    }
}
