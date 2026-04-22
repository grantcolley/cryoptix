using Cryoptix.Market.Data;

namespace Cryoptix.Market.Args
{
    public class TradeEventArgs
    {
        public IEnumerable<Trade>? Trades { get; set; }
    }
}