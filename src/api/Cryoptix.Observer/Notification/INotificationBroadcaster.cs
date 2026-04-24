namespace Cryoptix.Observer.Notification
{
    public interface INotificationBroadcaster
    {
        Task BroadcastAsync<TPayload>(
            MessageType messageType,
            TPayload payload,
            CancellationToken cancellationToken = default);
    }
}
