namespace Cryoptix.Strategy.Agent
{
    /// <summary>
    /// Defines the strategy agent contract.
    /// </summary>
    public interface IStrategyAgent : IAsyncDisposable
    {
        /// <summary>
        /// Starts the operation.
        /// </summary>
        /// <param name="strategy">The strategy.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task StartAsync(Strategies.Strategy strategy, CancellationToken cancellationToken);
        /// <summary>
        /// Stops the operation.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task StopAsync();
        /// <summary>
        /// Updates the operation.
        /// </summary>
        /// <param name="strategy">The strategy.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task UpdateAsync(Strategies.Strategy strategy);
    }
}
