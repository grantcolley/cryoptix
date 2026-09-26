namespace Cryoptix.Strategy.Indicators
{
    /// <summary>
    /// Represents a configured indicator for a strategy (for example SMA or EMA).
    /// </summary>
    public class Indicator
    {
        /// <summary>
        /// Gets or sets the display name for the indicator (for example "9 SMA").
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the numeric value for the indicator (typically the period).
        /// </summary>
        public int Value { get; set; }

        /// <summary>
        /// Gets the type of the indicator (SMA, EMA, etc.).
        /// </summary>
        public IndicatorType IndicatorType { get; init; }
    }
}
