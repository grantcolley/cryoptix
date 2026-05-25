namespace Cryoptix.Market.Strategy
{
    public class Indicators
    {
        public required DateTime TimestampUtc { get; init; }
        public required IReadOnlyDictionary<string, decimal> Values { get; init; }
    }
}
