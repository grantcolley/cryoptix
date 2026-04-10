using Cryoptix.Strategy.Clock;
using Cryoptix.Strategy.Event;
using Cryoptix.Strategy.Processor;

namespace Cryoptix.Strategy.Analysis
{
    public sealed class StrategyAnalysisContextFactory(
        IStrategyClock clock) : IStrategyAnalysisContextFactory
    {
        private readonly IStrategyClock _clock = clock;

        public StrategyAnalysisContext CreateForKline(
            StrategyProcessorSession session,
            KlineMarketEvent marketEvent)
        {
            ArgumentNullException.ThrowIfNull(session);
            ArgumentNullException.ThrowIfNull(marketEvent);

            return new StrategyAnalysisContext
            {
                Strategy = session.Strategy,
                ExchangeApi = session.ExchangeApi,
                Klines = session.Cache.GetKlines(session.Strategy.Symbol, session.Strategy.KlineInterval),
                Trades = session.Cache.GetTrades(session.Strategy.Symbol),
                CurrentEvent = new MarketEventEnvelope
                {
                    Kind = MarketEventKind.Kline,
                    Source = marketEvent.Source,
                    Kline = marketEvent.Kline
                },
                TimestampUtc = _clock.UtcNow
            };
        }

        public StrategyAnalysisContext CreateForTrade(
            StrategyProcessorSession session,
            TradeMarketEvent marketEvent)
        {
            ArgumentNullException.ThrowIfNull(session);
            ArgumentNullException.ThrowIfNull(marketEvent);

            return new StrategyAnalysisContext
            {
                Strategy = session.Strategy,
                ExchangeApi = session.ExchangeApi,
                Klines = session.Cache.GetKlines(session.Strategy.Symbol, session.Strategy.KlineInterval),
                Trades = session.Cache.GetTrades(session.Strategy.Symbol),
                CurrentEvent = new MarketEventEnvelope
                {
                    Kind = MarketEventKind.Trade,
                    Source = MarketEventSource.Live,
                    Trade = marketEvent.Trade
                },
                TimestampUtc = _clock.UtcNow
            };
        }
    }
}
