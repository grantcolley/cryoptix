using Cryoptix.Market.Models;

namespace Cryoptix.Market.Args
{
    public class TradeEventArgs
    {
        public IEnumerable<Trade>? Trades { get; set; }
    }
}