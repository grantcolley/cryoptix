using Cryoptix.Strategy.Processor;

namespace Cryoptix.Strategy.Cache
{
    public interface ITradingFlowSessionAccessor
    {
        void SetCurrent(StrategyProcessorSession session);
        void ClearCurrent();
        bool TryGetCurrent(out StrategyProcessorSession? session);
    }
}
