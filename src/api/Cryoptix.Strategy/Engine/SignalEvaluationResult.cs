namespace Cryoptix.Strategy.Engine
{
    public sealed class SignalEvaluationResult
    {
        public required Market.Strategy.Signal Signal { get; init; }

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
