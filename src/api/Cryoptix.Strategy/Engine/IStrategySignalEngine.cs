using Cryoptix.Strategy.Analysis;

namespace Cryoptix.Strategy.Engine
{
    public interface IStrategySignalEngine
    {
        Task<SignalEvaluationResult> EvaluateAsync(
            StrategyAnalysisContext context,
            IndicatorComputationResult indicators,
            CancellationToken cancellationToken);
    }
}
