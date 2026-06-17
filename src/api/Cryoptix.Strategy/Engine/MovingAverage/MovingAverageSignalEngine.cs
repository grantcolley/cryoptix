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
        /// <param name="indicatorsResult">Indicator values computed previously (SMA/EMA).</param>
        /// <param name="cancellationToken">Cancellation token to observe during evaluation.</param>
        /// <returns>A <see cref="SignalEvaluationResult"/> representing the evaluated signal.</returns>
        public Task<SignalEvaluationResult> EvaluateAsync(
            StrategyAnalysisContext context,
            IndicatorComputationResult indicatorsResult,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // TODO: Implement the moving average crossover logic here. For now, return a placeholder result indicating that the signal is not implemented.

            return Task.FromResult(SignalEvaluationResult.None(
                DateTime.UtcNow,
                "Signal not implemented yet."));

            //if (indicatorsResult.Indicators.TimestampUtc == DateTime.MinValue)
            //{
            //    return Task.FromResult(SignalEvaluationResult.None(
            //        DateTime.UtcNow,
            //        "Indicators unavailable."));
            //}

            //if (!indicatorsResult.Indicators.Values.TryGetValue("9 SMA", out decimal fast))
            //{
            //    return Task.FromResult(SignalEvaluationResult.None(
            //        DateTime.UtcNow,
            //        "9 SMA unavailable."));
            //}

            //if (!indicatorsResult.Indicators.Values.TryGetValue("21 SMA", out decimal slow))
            //{
            //    return Task.FromResult(SignalEvaluationResult.None(
            //        DateTime.UtcNow,
            //        "21 SMA unavailable."));
            //}

            //SignalType signalType = SignalType.None;
            //string reason = "No crossover.";

            //if (fast > slow)
            //{
            //    signalType = SignalType.Buy;
            //    reason = "9 SMA is above 21 SMA.";
            //}
            //else if (fast < slow)
            //{
            //    signalType = SignalType.Sell;
            //    reason = "9 SMA is below 21 SMA.";
            //}

            //_logger.LogDebug(
            //    "Evaluated signal for {Symbol}. Signal:{Signal} Reason:{Reason}",
            //    context.Strategy.Symbol,
            //    signalType,
            //    reason);

            //return Task.FromResult(new SignalEvaluationResult
            //{
            //    Signal = new Market.Strategy.Signal
            //    {
            //        TimestampUtc = indicatorsResult.Indicators.TimestampUtc,
            //        SignalType = signalType,
            //        Reason = reason
            //    },
            //});
        }
    }
}
