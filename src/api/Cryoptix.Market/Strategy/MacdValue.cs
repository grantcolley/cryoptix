namespace Cryoptix.Market.Strategy
{
    /// <summary>
    /// Represents the calculated values of a MACD indicator.
    /// </summary>
    public class MacdValue
    {
        /// <summary>
        /// Gets or sets the name of the MACD indicator, typically in the format "MACD(fastPeriod, slowPeriod, signalPeriod)".
        /// </summary>
        public string? Name { get; set; }

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
    }
}
