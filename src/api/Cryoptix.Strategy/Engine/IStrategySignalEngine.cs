using Cryoptix.Strategy.Analysis;

namespace Cryoptix.Strategy.Engine
{
    /// <summary>
    /// Defines the i strategy signal engine contract.
    /// </summary>
    public interface IStrategySignalEngine
    {
        /// <summary>
        /// Evaluates a trading signal using computed indicators and the analysis context.
        /// </summary>
        /// <param name="context">The analysis context containing strategy, market data and state.</param>
        /// <param name="indicators">Computed indicator values used for signal evaluation.</param>
        /// <param name="cancellationToken">A token to observe for cancellation.</param>
        /// <returns>A <see cref="SignalEvaluationResult"/> representing the evaluated signal and reason.</returns>
        Task<SignalEvaluationResult> EvaluateAsync(
            StrategyAnalysisContext context,
            IndicatorComputationResult indicators,
            CancellationToken cancellationToken);
    }
}
