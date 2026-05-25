using Cryoptix.Market.Strategy;
using Cryoptix.Strategy.Analysis;
using Microsoft.Extensions.Logging;

namespace Cryoptix.Strategy.Engine.MovingAverage
{
    public sealed class MovingAverageSignalEngine(
        ILogger<MovingAverageSignalEngine> logger) : IStrategySignalEngine
    {
        private readonly ILogger<MovingAverageSignalEngine> _logger = logger;

        /// <summary>
        /// Evaluates moving average crossover signals based on provided indicators.
        /// </summary>
        /// <param name="context">The analysis context containing strategy and market data.</param>
        /// <param name="indicators">Indicator values computed previously (SMA/EMA).</param>
        /// <param name="cancellationToken">Cancellation token to observe during evaluation.</param>
        /// <returns>A <see cref="SignalEvaluationResult"/> representing the evaluated signal.</returns>
        public Task<SignalEvaluationResult> EvaluateAsync(
            StrategyAnalysisContext context,
            IndicatorComputationResult indicators,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!indicators.Indicators.Values.TryGetValue("SMA_FAST", out decimal fast))
            {
                return Task.FromResult(SignalEvaluationResult.None(
                    DateTime.UtcNow,
                    "Fast SMA unavailable."));
            }

            if (!indicators.Indicators.Values.TryGetValue("SMA_SLOW", out decimal slow))
            {
                return Task.FromResult(SignalEvaluationResult.None(
                    DateTime.UtcNow,
                    "Slow SMA unavailable."));
            }

            SignalType signalType = SignalType.None;
            string reason = "No crossover.";

            if (fast > slow)
            {
                signalType = SignalType.Buy;
                reason = "Fast SMA is above Slow SMA.";
            }
            else if (fast < slow)
            {
                signalType = SignalType.Sell;
                reason = "Fast SMA is below Slow SMA.";
            }

            _logger.LogDebug(
                "Evaluated signal for {Symbol}. Signal:{Signal} Reason:{Reason}",
                context.Strategy.Symbol,
                signalType,
                reason);

            return Task.FromResult(new SignalEvaluationResult
            {
                Signal = new Market.Strategy.Signal
                {
                    TimestampUtc = DateTime.UtcNow,
                    SignalType = signalType,
                    Reason = reason
                },
            });
        }
    }
}
