namespace Cryoptix.Observer.Subscription
{
    /// <summary>
    /// Defines the subscription manager contract.
    /// </summary>
    public interface ISubscriptionManager
    {
        /// <summary>
        /// Registers the operation.
        /// </summary>
        /// <param name="subscriber">The subscriber.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task RegisterAsync(SubscriberConnection subscriber, CancellationToken cancellationToken = default);
        /// <summary>
        /// Unregisters the operation.
        /// </summary>
        /// <param name="connectionId">The connection ID.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task UnregisterAsync(string connectionId, CancellationToken cancellationToken = default);
        /// <summary>
        /// Gets the all.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task<IReadOnlyCollection<SubscriberConnection>> GetAllAsync(CancellationToken cancellationToken = default);
        /// <summary>
        /// Gets a value indicating whether registered.
        /// </summary>
        /// <param name="connectionId">The connection ID.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task<bool> IsRegisteredAsync(string connectionId, CancellationToken cancellationToken = default);
    }
}
