using Cryoptix.Market.Data;

namespace Cryoptix.Strategy.Event
{
    public sealed class KlineMarketEvent(Kline kline, MarketEventSource source) : MarketEvent
    {
        public Kline Kline { get; } = kline ?? throw new ArgumentNullException(nameof(kline));
        public MarketEventSource Source { get; } = source;
    }
}
