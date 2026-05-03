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
                    Symbol = string.Empty,
                    Interval = default,
                    SnapshotTimeUtc = _strategyClock.UtcNow,
                    Klines = [],
                    Trades = []
                });
            }

            Strategies.Strategy strategy = session.Strategy;
            string symbol = strategy.Symbol ?? string.Empty;
            KlineInterval interval = strategy.KlineInterval;

            return Task.FromResult(new MarketDataSnapshot
            {
                Symbol = symbol,
                Interval = interval,
                SnapshotTimeUtc = _strategyClock.UtcNow,
                Klines = [.. session.Cache.GetKlines(symbol, interval)],
                Trades = [.. session.Cache.GetTrades(symbol)]
            });
        }
    }
}