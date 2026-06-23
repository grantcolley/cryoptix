using Cryoptix.Strategy.Analysis;
using Cryoptix.Strategy.Engine;

namespace Cryoptix.Strategy.Signal
{
    /// <summary>
    /// Defines the i strategy signal handler contract.
    /// </summary>
    public interface IStrategySignalHandler
    {
        Task HandleAsync(
            StrategyAnalysisContext context,
            SignalEvaluationResult signalEvaluationResult,
            CancellationToken cancellationToken);
    }
}
