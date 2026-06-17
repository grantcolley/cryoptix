using Cryoptix.Market.Data;

namespace Cryoptix.Strategy.Event
{
    public sealed class TradeMarketEvent(Trade trade) : MarketEvent
    {
        public Trade Trade { get; } = trade ?? throw new ArgumentNullException(nameof(trade));
    }
}
