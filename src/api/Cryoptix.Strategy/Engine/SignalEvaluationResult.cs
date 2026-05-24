namespace Cryoptix.Strategy.Engine
{
    public sealed class SignalEvaluationResult
    {
        public required DateTime TimestampUtc { get; init; }
        public required StrategySignal Signal { get; init; }
        public string? Reason { get; init; }

        public static SignalEvaluationResult None(DateTime timestampUtc, string? reason = null) =>
            new()
            {
                TimestampUtc = timestampUtc,
                Signal = StrategySignal.None,
                Reason = reason
            };
    }
}
