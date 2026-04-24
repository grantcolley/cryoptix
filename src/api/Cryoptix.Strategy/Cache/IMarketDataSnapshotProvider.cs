namespace Cryoptix.Strategy.Cache
{
    public interface IMarketDataSnapshotProvider
    {
        Task<MarketDataSnapshot> GetSnapshotAsync(CancellationToken cancellationToken = default);
    }
}