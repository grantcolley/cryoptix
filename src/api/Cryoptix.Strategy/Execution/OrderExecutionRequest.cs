using Cryoptix.Exchange.Models;
using Cryoptix.Strategy.Analysis;
using Cryoptix.Strategy.Engine;

namespace Cryoptix.Strategy.Execution
{
    public sealed class OrderExecutionRequest
    {
        public required StrategyAnalysisContext Context { get; init; }
        public required SignalEvaluationResult Signal { get; init; }
        public required OrderBook OrderBook { get; init; }
        public required Account Account { get; init; }

        /// <summary>
        /// Asset being bought or sold, derived from the strategy symbol if needed.
        /// Example: BTC for BTCUSDT.
        /// </summary>
        public string? BaseAsset { get; init; }

        /// <summary>
        /// Quote asset used for pricing and cash balance checks.
        /// Example: USDT for BTCUSDT.
        /// </summary>
        public string? QuoteAsset { get; init; }

        /// <summary>
        /// Optional quantity computed by the signal handler/risk layer.
        /// </summary>
        public decimal? Quantity { get; init; }

        /// <summary>
        /// Optional limit price hint. Null means market-style execution or executor-side pricing.
        /// </summary>
        public decimal? LimitPrice { get; init; }
    }
}
