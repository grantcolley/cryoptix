using Cryoptix.Market.Models;

namespace Cryoptix.Market.Args
{
    public class StatisticsEventArgs
    {
        public IEnumerable<SymbolStats>? Statistics { get; set; }
    }
}