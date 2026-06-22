namespace Cryoptix.Market.Strategy
{
    /// <summary>
    /// Represents the signal.
    /// </summary>
    public class Signal
    {
        /// <summary>
        /// Gets or sets the timestamp utc.
        /// </summary>
        public required DateTime TimestampUtc { get; init; }
        /// <summary>
        /// Gets or sets the signal type.
        /// </summary>
        public required SignalType SignalType { get; init; }
        /// <summary>
        /// Gets or sets the reason.
        /// </summary>
        public string? Reason { get; init; }
    }
}
