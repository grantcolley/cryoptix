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
        /// Gets or sets the values.
        /// </summary>
        public required IReadOnlyDictionary<string, decimal> Values { get; init; }
    }
}
