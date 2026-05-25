namespace Cryoptix.Strategy.Engine
{
    public sealed class IndicatorComputationResult
    {
        public required Market.Strategy.Indicators Indicators { get; init; }

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
