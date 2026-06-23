namespace Cryoptix.Strategy.Cache
{
    /// <summary>
    /// Defines the market data snapshot provider contract.
    /// </summary>
    public interface IMarketDataSnapshotProvider
    {
        /// <summary>
        /// Gets the snapshot.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task<MarketDataSnapshot> GetSnapshotAsync(CancellationToken cancellationToken = default);
    }
}