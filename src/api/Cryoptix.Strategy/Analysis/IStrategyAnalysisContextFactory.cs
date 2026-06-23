using Cryoptix.Strategy.Event;
using Cryoptix.Strategy.Processor;

namespace Cryoptix.Strategy.Analysis
{
    /// <summary>
    /// Defines the strategy analysis context factory contract.
    /// </summary>
    public interface IStrategyAnalysisContextFactory
    {
        /// <summary>
        /// Creates the for kline.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="marketEvent">The market event.</param>
        /// <returns>The create for kline result.</returns>
        StrategyAnalysisContext CreateForKline(StrategyProcessorSession session, KlineMarketEvent marketEvent);
        /// <summary>
        /// Creates the for trade.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="marketEvent">The market event.</param>
        /// <returns>The create for trade result.</returns>
        StrategyAnalysisContext CreateForTrade(StrategyProcessorSession session, TradeMarketEvent marketEvent);
    }
}
