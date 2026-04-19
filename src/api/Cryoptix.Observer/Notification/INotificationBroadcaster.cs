namespace Cryoptix.Observer.Notification
{
    public interface INotificationBroadcaster
    {
        Task BroadcastAsync(
            string messageType,
            string payloadJson,
            CancellationToken cancellationToken = default);
    }
}
