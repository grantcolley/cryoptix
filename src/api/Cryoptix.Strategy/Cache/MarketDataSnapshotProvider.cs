using Cryoptix.Market.Data;
using Cryoptix.Strategy.Clock;
using Cryoptix.Strategy.Processor;

namespace Cryoptix.Strategy.Cache
{
    /// <summary>
    /// Represents the market data snapshot provider.
    /// </summary>
    public sealed class MarketDataSnapshotProvider(
        IMarketEventSessionAccessor marketEventSessionAccessor,
        IStrategyClock strategyClock) : IMarketDataSnapshotProvider
    {
        private readonly IMarketEventSessionAccessor _marketEventSessionAccessor = marketEventSessionAccessor;
        private readonly IStrategyClock _strategyClock = strategyClock;

        /// <summary>
        /// Executes the get snapshot async operation.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token value.</param>
        /// <returns>The get snapshot async result.</returns>
        public Task<MarketDataSnapshot> GetSnapshotAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!_marketEventSessionAccessor.TryGetCurrent(out StrategyProcessorSession? session) || session == null)
            {
                return Task.FromResult(new MarketDataSnapshot
                {
                    Strategy = new Strategies.Strategy(),
                    SnapshotTimeUtc = _strategyClock.UtcNow,
                    Symbol = new Symbol(),
                    Klines = [],
                    Trades = [],
                    Indicators = [],
                    Signals = []
                });
            }

            Strategies.Strategy strategy = session.Strategy;

            return Task.FromResult(new MarketDataSnapshot
            {
                Strategy = strategy,
                SnapshotTimeUtc = _strategyClock.UtcNow,
                Symbol = session.Cache.GetSymbolForStrategy(strategy.Symbol ?? string.Empty) ?? new Symbol(),
                Klines = [.. session.Cache.GetKlines(strategy.Symbol ?? string.Empty, strategy.KlineInterval)],
                Trades = [.. session.Cache.GetTrades(strategy.Symbol ?? string.Empty)],
                Indicators = [.. session.Cache.GetIndicators(strategy.Symbol ?? string.Empty)],
                Signals = [.. session.Cache.GetSignals(strategy.Symbol ?? string.Empty)]
            });
        }
    }
}