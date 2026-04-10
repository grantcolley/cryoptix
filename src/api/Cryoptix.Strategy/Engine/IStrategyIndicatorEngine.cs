using Cryoptix.Strategy.Analysis;

namespace Cryoptix.Strategy.Engine
{
    public interface IStrategyIndicatorEngine
    {
        Task<IndicatorComputationResult> ComputeAsync(
            StrategyAnalysisContext context,
            CancellationToken cancellationToken);
    }
}
