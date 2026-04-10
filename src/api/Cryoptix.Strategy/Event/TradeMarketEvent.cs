using Cryoptix.Exchange.Models;

namespace Cryoptix.Strategy.Event
{
    public sealed class TradeMarketEvent : MarketEvent
    {
        public TradeMarketEvent(Trade trade)
        {
            Trade = trade ?? throw new ArgumentNullException(nameof(trade));
        }

        public Trade Trade { get; }
    }
}
