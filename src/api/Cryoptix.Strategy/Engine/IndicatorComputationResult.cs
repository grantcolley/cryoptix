using Cryoptix.Strategy.Indicators;

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
        /// Gets or sets the EMA (Exponential Moving Average) computation results for one
        /// or more periods. Engines should populate this collection with the EMA
        /// snapshots they computed during the indicator calculation pass.
        /// </summary>
        public required IReadOnlyList<Ema> Emas { get; init; }

        /// <summary>
        /// Gets or sets the RSI (Relative Strength Index) computation results for one
        /// or more periods. Engines should populate this collection with the RSI
        /// snapshots they computed during the indicator calculation pass.
        /// </summary>
        public required IReadOnlyList<Rsi> Rsis { get; init; }

        /// <summary>
        /// Executes the empty operation.
        /// </summary>
        /// <param name="timestampUtc">The timestamp utc value.</param>
        /// <returns>The empty result.</returns>
        public static IndicatorComputationResult Empty(DateTime timestampUtc) =>
            new()
            {
                Rsis = [],
                Indicators = new Market.Strategy.Indicators
                {
                    TimestampUtc = timestampUtc,
                    Series = new Dictionary<string, decimal>(),
                    Snapshots = new Dictionary<string, decimal>()
                }
            };
    }
}
