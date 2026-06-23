namespace Cryoptix.Strategy.Engine
{
    /// <summary>
    /// Represents the signal evaluation result.
    /// </summary>
    public sealed class SignalEvaluationResult
    {
        /// <summary>
        /// Gets or sets the signal.
        /// </summary>
        public required Market.Strategy.Signal Signal { get; init; }

        /// <summary>
        /// Executes the none operation.
        /// </summary>
        /// <param name="timestampUtc">The timestamp utc value.</param>
        /// <param name="reason">The reason value.</param>
        /// <returns>The none result.</returns>
        public static SignalEvaluationResult None(DateTime timestampUtc, string? reason = null) =>
            new()
            {
                Signal = new Market.Strategy.Signal
                {
                    TimestampUtc = timestampUtc,
                    SignalType = Market.Strategy.SignalType.None,
                    Reason = reason
                },
            };
    }
}
