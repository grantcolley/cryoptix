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
            return new StrategyAnalysisContext
            {
                Strategy = session.Strategy,
                ExchangeApi = session.ExchangeApi,
                Credentials = session.Credentials,
                Klines = session.Cache.GetKlines(session.Strategy.Symbol!, session.Strategy.KlineInterval),
                Trades = session.Cache.GetTrades(session.Strategy.Symbol!),
                CurrentEvent = new MarketEventEnvelope
                {
                    Kind = MarketEventKind.Kline,
                    Source = marketEvent.Source,
                    Kline = marketEvent.Kline
                },
                OrderBookRealtimeState = session.OrderBookRealtimeState,
                AccountRealtimeState = session.AccountRealtimeState,
                TimestampUtc = _clock.UtcNow
            };
        }

        public StrategyAnalysisContext CreateForTrade(
            StrategyProcessorSession session,
            TradeMarketEvent marketEvent)
        {
            return new StrategyAnalysisContext
            {
                Strategy = session.Strategy,
                ExchangeApi = session.ExchangeApi,
                Credentials = session.Credentials,
                Klines = session.Cache.GetKlines(session.Strategy.Symbol!, session.Strategy.KlineInterval),
                Trades = session.Cache.GetTrades(session.Strategy.Symbol!),
                CurrentEvent = new MarketEventEnvelope
                {
                    Kind = MarketEventKind.Trade,
                    Source = MarketEventSource.Live,
                    Trade = marketEvent.Trade
                },
                OrderBookRealtimeState = session.OrderBookRealtimeState,
                AccountRealtimeState = session.AccountRealtimeState,
                TimestampUtc = _clock.UtcNow
            };
        }
    }
}
