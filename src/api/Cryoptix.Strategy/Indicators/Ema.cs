namespace Cryoptix.Strategy.Indicators
{
    /// <summary>
    /// Represents a computed Exponential Moving Average (EMA) snapshot for a symbol.
    /// </summary>
    public sealed class Ema
    {
        /// <summary>
        /// Gets or sets the EMA period used in the calculation.
        /// </summary>
        public int Period { get; set; }

        /// <summary>
        /// Gets or sets the current EMA value.
        /// </summary>
        public decimal Value { get; set; }

        /// <summary>
        /// Gets or sets the timestamp (UTC) associated with this EMA snapshot.
        /// </summary>
        public required DateTime TimestampUtc { get; set; }
    }
}
