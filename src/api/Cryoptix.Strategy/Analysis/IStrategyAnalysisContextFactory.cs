using Cryoptix.Strategy.Event;
using Cryoptix.Strategy.Processor;

namespace Cryoptix.Strategy.Analysis
{
    public interface IStrategyAnalysisContextFactory
    {
        StrategyAnalysisContext CreateForKline(StrategyProcessorSession session, KlineMarketEvent marketEvent);
        StrategyAnalysisContext CreateForTrade(StrategyProcessorSession session, TradeMarketEvent marketEvent);
    }
}
