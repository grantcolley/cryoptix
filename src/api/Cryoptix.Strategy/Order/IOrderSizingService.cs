using Cryoptix.Market.Models;
using Cryoptix.Strategy.Analysis;
using Cryoptix.Strategy.Engine;

namespace Cryoptix.Strategy.Order
{
    public interface IOrderSizingService
    {
        OrderSizingResult? Size(
            StrategyAnalysisContext context,
            SignalEvaluationResult signal,
            OrderBook orderBook,
            Account account);
    }
}
