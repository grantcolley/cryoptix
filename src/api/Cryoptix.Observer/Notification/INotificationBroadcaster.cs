namespace Cryoptix.Observer.Notification
{
    /// <summary>
    /// Defines the i notification broadcaster contract.
    /// </summary>
    public interface INotificationBroadcaster
    {
        Task BroadcastAsync<TPayload>(
            MessageType messageType,
            TPayload payload,
            CancellationToken cancellationToken = default);
    }
}
