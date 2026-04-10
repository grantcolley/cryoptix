namespace Cryoptix.Strategy.Engine
{
    public sealed class IndicatorComputationResult
    {
        public required DateTime TimestampUtc { get; init; }
        public required IReadOnlyDictionary<string, decimal> Values { get; init; }

        public static IndicatorComputationResult Empty(DateTime timestampUtc) =>
            new()
            {
                TimestampUtc = timestampUtc,
                Values = new Dictionary<string, decimal>()
            };
    }
}
