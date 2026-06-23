using Cryoptix.Strategy.Processor;

namespace Cryoptix.Strategy.Cache
{
    /// <summary>
    /// Defines the i trading flow session accessor contract.
    /// </summary>
    public interface ITradingFlowSessionAccessor
    {
        void SetCurrent(StrategyProcessorSession session);
        void ClearCurrent();
        bool TryGetCurrent(out StrategyProcessorSession? session);
    }
}
