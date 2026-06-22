using Cryoptix.Market.Data;

namespace Cryoptix.Market.Args
{
    /// <summary>
    /// Represents the statistics event args.
    /// </summary>
    public class StatisticsEventArgs
    {
        /// <summary>
        /// Gets or sets the statistics.
        /// </summary>
        public IEnumerable<SymbolStats>? Statistics { get; set; }
    }
}