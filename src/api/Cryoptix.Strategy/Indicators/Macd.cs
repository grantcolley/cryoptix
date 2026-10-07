namespace Cryoptix.Strategy.Indicators
{
    /// <summary>
    /// Represents a computed Moving Average Convergence Divergence (MACD) snapshot for a symbol.
    /// </summary>
    public sealed class Macd
    {
        /// <summary>
        /// Gets or sets the fast EMA period used in the MACD calculation.
        /// </summary>
        public int FastPeriod { get; set; }

        /// <summary>
        /// Gets or sets the slow EMA period used in the MACD calculation.
        /// </summary>
        public int SlowPeriod { get; set; }

        /// <summary>
        /// Gets or sets the signal EMA period used in the MACD calculation.
        /// </summary>
        public int SignalPeriod { get; set; }

        /// <summary>
        /// Gets or sets the current fast exponential moving average value.
        /// </summary>
        public decimal FastEma { get; set; }

        /// <summary>
        /// Gets or sets the current slow exponential moving average value.
        /// </summary>
        public decimal SlowEma { get; set; }

        /// <summary>
        /// Gets or sets the MACD value, calculated as the fast EMA minus the slow EMA.
        /// </summary>
        public decimal Value { get; set; }

        /// <summary>
        /// Gets or sets the signal line value, calculated as an EMA of the MACD value.
        /// </summary>
        public decimal Signal { get; set; }

        /// <summary>
        /// Gets or sets the MACD histogram value, calculated as the MACD value minus the signal line.
        /// </summary>
        public decimal Histogram { get; set; }

        /// <summary>
        /// Gets or sets the timestamp (UTC) associated with this MACD snapshot.
        /// </summary>
        public required DateTime TimestampUtc { get; set; }
    }
}
