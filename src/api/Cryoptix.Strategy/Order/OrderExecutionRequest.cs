using Cryoptix.Market.Models;
using Cryoptix.Strategy.Analysis;
using Cryoptix.Strategy.Engine;

namespace Cryoptix.Strategy.Order
{
    public sealed class OrderExecutionRequest
    {
        public required StrategyAnalysisContext Context { get; init; }
        public required SignalEvaluationResult Signal { get; init; }
        public required OrderBook OrderBook { get; init; }
        public required Account Account { get; init; }
        public required OrderSide Side { get; init; }
        public required string Symbol { get; init; }
        public required string BaseAsset { get; init; }
        public required string QuoteAsset { get; init; }
        public required decimal Quantity { get; init; }
        public decimal? LimitPrice { get; init; }
        public decimal? QuoteNotional { get; init; }
        public string? Reason { get; init; }
    }
}
