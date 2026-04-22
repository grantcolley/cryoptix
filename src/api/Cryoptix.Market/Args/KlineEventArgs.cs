using Cryoptix.Market.Models;

namespace Cryoptix.Market.Args
{
    public class KlineEventArgs
    {
        public IEnumerable<Kline>? Klines { get; set; }
    }
}