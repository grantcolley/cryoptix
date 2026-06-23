namespace Cryoptix.Strategy.Notification
{
    /// <summary>
    /// Defines the strategy status notifier contract.
    /// </summary>
    public interface IStrategyStatusNotifier
    {
        /// <summary>
        /// Notifies the started.
        /// </summary>
        /// <param name="strategy">The strategy.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task NotifyStartedAsync(Strategies.Strategy strategy, CancellationToken cancellationToken);
        /// <summary>
        /// Notifies the market data snapshot.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task NotifyMarketDataSnapshotAsync(CancellationToken cancellationToken);
        /// <summary>
        /// Notifies the updated.
        /// </summary>
        /// <param name="strategy">The strategy.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task NotifyUpdatedAsync(Strategies.Strategy strategy, CancellationToken cancellationToken);
    }
}
