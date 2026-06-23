using Cryoptix.Strategy.Processor;

namespace Cryoptix.Strategy.Cache
{
    /// <summary>
    /// Defines the trading flow session accessor contract.
    /// </summary>
    public interface ITradingFlowSessionAccessor
    {
        /// <summary>
        /// Sets the current.
        /// </summary>
        /// <param name="session">The session.</param>
        void SetCurrent(StrategyProcessorSession session);
        /// <summary>
        /// Clears the current.
        /// </summary>
        void ClearCurrent();
        /// <summary>
        /// Tries to get current.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <returns>true if the operation succeeds; otherwise, false.</returns>
        bool TryGetCurrent(out StrategyProcessorSession? session);
    }
}
