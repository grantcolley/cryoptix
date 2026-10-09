namespace Cryoptix.Strategy.Indicators
{
    /// <summary>
    /// Represents an integer parameter of a configured indicator.
    /// </summary>
    public sealed class IndicatorValue
    {
        /// <summary>
        /// Gets or sets the parameter type.
        /// </summary>
        public IndicatorValueType Type { get; set; }

        /// <summary>
        /// Gets or sets the integer parameter value.
        /// </summary>
        public int Value { get; set; }
    }
}
