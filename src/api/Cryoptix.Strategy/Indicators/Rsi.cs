namespace Cryoptix.Strategy.Indicators
{
    /// <summary>
    /// Represents a computed Relative Strength Index (RSI) snapshot for a symbol.
    /// </summary>
    public sealed class Rsi
    {
        /// <summary>
        /// Gets or sets the RSI period used in the calculation.
        /// </summary>
        public int Period { get; set; }

        /// <summary>
        /// Gets or sets the previous close price used when updating the RSI.
        /// </summary>
        public decimal PreviousClose { get; set; }

        /// <summary>
        /// Gets or sets the average gain used in the RSI calculation.
        /// </summary>
        public decimal AverageGain { get; set; }

        /// <summary>
        /// Gets or sets the average loss used in the RSI calculation.
        /// </summary>
        public decimal AverageLoss { get; set; }

        /// <summary>
        /// Gets or sets the RSI value scaled 0-100.
        /// </summary>
        public decimal Value { get; set; }

        /// <summary>
        /// Gets or sets the timestamp (UTC) associated with this RSI snapshot.
        /// </summary>
        public required DateTime TimestampUtc { get; set; }
    }
}
