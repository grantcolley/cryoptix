namespace Cryoptix.Strategy.Cache
{
    /// <summary>
    /// Defines the i market data snapshot provider contract.
    /// </summary>
    public interface IMarketDataSnapshotProvider
    {
        Task<MarketDataSnapshot> GetSnapshotAsync(CancellationToken cancellationToken = default);
    }
}