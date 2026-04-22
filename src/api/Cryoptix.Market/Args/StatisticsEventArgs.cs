using Cryoptix.Market.Data;

namespace Cryoptix.Market.Args
{
    public class StatisticsEventArgs
    {
        public IEnumerable<SymbolStats>? Statistics { get; set; }
    }
}