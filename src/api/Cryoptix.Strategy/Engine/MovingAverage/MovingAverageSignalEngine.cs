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

            if (!indicators.Values.TryGetValue("SMA_FAST", out decimal fast))
            {
                return Task.FromResult(SignalEvaluationResult.None(
                    DateTime.UtcNow,
                    "Fast SMA unavailable."));
            }

            if (!indicators.Values.TryGetValue("SMA_SLOW", out decimal slow))
            {
                return Task.FromResult(SignalEvaluationResult.None(
                    DateTime.UtcNow,
                    "Slow SMA unavailable."));
            }

            StrategySignal signal = StrategySignal.None;
            string reason = "No crossover.";

            if (fast > slow)
            {
                signal = StrategySignal.Buy;
                reason = "Fast SMA is above Slow SMA.";
            }
            else if (fast < slow)
            {
                signal = StrategySignal.Sell;
                reason = "Fast SMA is below Slow SMA.";
            }

            _logger.LogDebug(
                "Evaluated signal for {Symbol}. Signal:{Signal} Reason:{Reason}",
                context.Strategy.Symbol,
                signal,
                reason);

            return Task.FromResult(new SignalEvaluationResult
            {
                TimestampUtc = DateTime.UtcNow,
                Signal = signal,
                Reason = reason
            });
        }
    }
}
