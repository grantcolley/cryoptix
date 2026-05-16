using Cryoptix.Strategy.Clock;
using Cryoptix.Strategy.Processor;
using Cryoptix.Strategy.State;

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
                    StrategyState = StrategyState.Idle,
                    Strategy = new Strategies.Strategy(),
                    SnapshotTimeUtc = _strategyClock.UtcNow,
                    Klines = [],
                    Trades = []
                });
            }

            Strategies.Strategy strategy = session.Strategy;

            return Task.FromResult(new MarketDataSnapshot
            {
                StrategyState = StrategyState.Running,
                Strategy = strategy,
                SnapshotTimeUtc = _strategyClock.UtcNow,
                Klines = [.. session.Cache.GetKlines(strategy.Symbol ?? string.Empty, strategy.KlineInterval)],
                Trades = [.. session.Cache.GetTrades(strategy.Symbol ?? string.Empty)]
            });
        }
    }
}