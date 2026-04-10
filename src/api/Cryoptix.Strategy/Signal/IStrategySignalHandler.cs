using Cryoptix.Strategy.Analysis;
using Cryoptix.Strategy.Engine;

namespace Cryoptix.Strategy.Signal
{
    public interface IStrategySignalHandler
    {
        Task HandleAsync(
            StrategyAnalysisContext context,
            SignalEvaluationResult signal,
            CancellationToken cancellationToken);
    }
}
