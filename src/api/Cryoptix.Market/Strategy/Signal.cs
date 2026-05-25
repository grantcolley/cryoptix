namespace Cryoptix.Market.Strategy
{
    public class Signal
    {
        public required DateTime TimestampUtc { get; init; }
        public required SignalType SignalType { get; init; }
        public string? Reason { get; init; }
    }
}
