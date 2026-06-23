namespace Cryoptix.Observer.Notification
{
    /// <summary>
    /// Defines the notification broadcaster contract.
    /// </summary>
    public interface INotificationBroadcaster
    {
        /// <summary>
        /// Executes the broadcast async<tpayload> operation.
        /// </summary>
        /// <param name="messageType">The message type.</param>
        /// <param name="payload">The payload.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task BroadcastAsync<TPayload>(
            /// <summary>
            /// Gets the value.
            /// </summary>
            MessageType messageType,
            /// <summary>
            /// Gets the value.
            /// </summary>
            TPayload payload,
            /// <summary>
            /// Gets the value.
            /// </summary>
            CancellationToken cancellationToken = default);
    }
}
