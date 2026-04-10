using Cryoptix.Exchange.Models;

namespace Cryoptix.Strategy.Event
{
    public sealed class KlineMarketEvent : MarketEvent
    {
        public KlineMarketEvent(Kline kline, MarketEventSource source)
        {
            Kline = kline ?? throw new ArgumentNullException(nameof(kline));
            Source = source;
        }

        public Kline Kline { get; }
        public MarketEventSource Source { get; }
    }
}
