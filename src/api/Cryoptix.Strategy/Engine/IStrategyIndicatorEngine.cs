using Cryoptix.Strategy.Analysis;

namespace Cryoptix.Strategy.Engine
{
    /// <summary>
    /// Defines the strategy indicator engine contract.
    /// </summary>
    public interface IStrategyIndicatorEngine
    {
        /// <summary>
        /// Computes indicator values for the provided analysis context.
        /// </summary>
        /// <param name="context">The analysis context containing strategy, market data and state.</param>
        /// <param name="cancellationToken">A token to observe for cancellation.</param>
        /// <returns>An <see cref="IndicatorComputationResult"/> containing calculated indicator values.</returns>
        Task<IndicatorComputationResult> ComputeAsync(
            /// <summary>
            /// Gets the value.
            /// </summary>
            StrategyAnalysisContext context,
            /// <summary>
            /// Gets the value.
            /// </summary>
            CancellationToken cancellationToken);
    }
}
