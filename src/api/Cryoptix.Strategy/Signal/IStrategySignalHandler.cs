using Cryoptix.Strategy.Analysis;
using Cryoptix.Strategy.Engine;

namespace Cryoptix.Strategy.Signal
{
    /// <summary>
    /// Defines the strategy signal handler contract.
    /// </summary>
    public interface IStrategySignalHandler
    {
        /// <summary>
        /// Handles the operation.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="signalEvaluationResult">The signal evaluation result.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task HandleAsync(
            /// <summary>
            /// Gets the value.
            /// </summary>
            StrategyAnalysisContext context,
            /// <summary>
            /// Gets the value.
            /// </summary>
            SignalEvaluationResult signalEvaluationResult,
            /// <summary>
            /// Gets the value.
            /// </summary>
            CancellationToken cancellationToken);
    }
}
