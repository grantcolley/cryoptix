namespace Cryoptix.Strategy.Engine
{
    /// <summary>
    /// Represents the indicator computation result.
    /// </summary>
    public sealed class IndicatorComputationResult
    {
        /// <summary>
        /// Gets or sets the indicators.
        /// </summary>
        public required Market.Strategy.Indicators Indicators { get; init; }

        /// <summary>
        /// Executes the empty operation.
        /// </summary>
        /// <param name="timestampUtc">The timestamp utc value.</param>
        /// <returns>The empty result.</returns>
        public static IndicatorComputationResult Empty(DateTime timestampUtc) =>
            new()
            {
                Indicators = new Market.Strategy.Indicators
                {
                    TimestampUtc = timestampUtc,
                    Values = new Dictionary<string, decimal>()
                }
            };
    }
}
