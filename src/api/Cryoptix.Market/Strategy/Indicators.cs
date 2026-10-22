namespace Cryoptix.Market.Strategy
{
    /// <summary>
    /// Represents the indicators.
    /// </summary>
    public class Indicators
    {
        /// <summary>
        /// Gets or sets the timestamp utc.
        /// </summary>
        public required DateTime TimestampUtc { get; init; }
        /// <summary>
        /// Gets or sets the calculated moving average values by indicator name.
        /// </summary>
        public required IReadOnlyDictionary<string, decimal> Mas { get; init; }
        /// <summary>
        /// Gets or sets the calculated RSI values by indicator name.
        /// </summary>
        public required IReadOnlyDictionary<string, decimal> Rsis { get; init; }
        /// <summary>
        /// Gets or sets the calculated MACD values by indicator name.
        /// </summary>
        public required IReadOnlyDictionary<string, MacdValue> Macds { get; init; }
    }
}
