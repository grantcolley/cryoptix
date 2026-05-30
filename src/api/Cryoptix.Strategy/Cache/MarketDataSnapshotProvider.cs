using Cryoptix.Market.Data;
using Cryoptix.Strategy.Clock;
using Cryoptix.Strategy.Processor;

namespace Cryoptix.Strategy.Cache
{
    public sealed class MarketDataSnapshotProvider(
        ITradingFlowSessionAccessor tradingFlowSessionAccessor,
        IStrategyClock strategyClock) : IMarketDataSnapshotProvider
    {
        private readonly ITradingFlowSessionAccessor _tradingFlowSessionAccessor = tradingFlowSessionAccessor;
        private readonly IStrategyClock _strategyClock = strategyClock;

        public Task<MarketDataSnapshot> GetSnapshotAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!_tradingFlowSessionAccessor.TryGetCurrent(out StrategyProcessorSession? session) || session == null)
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