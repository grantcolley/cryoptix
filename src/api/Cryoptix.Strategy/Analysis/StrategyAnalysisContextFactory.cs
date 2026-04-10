using Cryoptix.Strategy.Event;
using Cryoptix.Strategy.Processor;

namespace Cryoptix.Strategy.Analysis
{
    public sealed class StrategyAnalysisContextFactory : IStrategyAnalysisContextFactory
    {
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
                TimestampUtc = DateTime.UtcNow
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
                TimestampUtc = DateTime.UtcNow
            };
        }
    }
}
