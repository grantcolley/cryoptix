using Cryoptix.Strategy.Event;
using Cryoptix.Strategy.Processor;

namespace Cryoptix.Strategy.Analysis
{
    /// <summary>
    /// Defines the i strategy analysis context factory contract.
    /// </summary>
    public interface IStrategyAnalysisContextFactory
    {
        StrategyAnalysisContext CreateForKline(StrategyProcessorSession session, KlineMarketEvent marketEvent);
        StrategyAnalysisContext CreateForTrade(StrategyProcessorSession session, TradeMarketEvent marketEvent);
    }
}
