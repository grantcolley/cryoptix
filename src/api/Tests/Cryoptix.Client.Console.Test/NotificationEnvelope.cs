namespace Cryoptix.Client.Console.Test
{
    public sealed class NotificationEnvelope
    {
        public string MessageType { get; set; } = default!;
        public DateTime TimestampUtc { get; set; }
        public object? Payload { get; set; } = default!;
    }
}
