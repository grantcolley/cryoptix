using Cryoptix.Market.Data;

namespace Cryoptix.Market.Args
{
    /// <summary>
    /// Represents the kline event args.
    /// </summary>
    public class KlineEventArgs
    {
        /// <summary>
        /// Gets or sets the klines.
        /// </summary>
        public IEnumerable<Kline>? Klines { get; set; }
    }
}