using Cryoptix.Market.Data;
using Cryoptix.Strategy.Analysis;
using Cryoptix.Strategy.Engine;

namespace Cryoptix.Strategy.Order
{
    /// <summary>
    /// Represents the order execution request.
    /// </summary>
    public sealed class OrderExecutionRequest
    {
        /// <summary>
        /// Gets or sets the context.
        /// </summary>
        public required StrategyAnalysisContext Context { get; init; }
        /// <summary>
        /// Gets or sets the signal.
        /// </summary>
        public required SignalEvaluationResult Signal { get; init; }
        /// <summary>
        /// Gets or sets the order book.
        /// </summary>
        public required OrderBook OrderBook { get; init; }
        /// <summary>
        /// Gets or sets the account.
        /// </summary>
        public required Account Account { get; init; }
        /// <summary>
        /// Gets or sets the side.
        /// </summary>
        public required OrderSide Side { get; init; }
        /// <summary>
        /// Gets or sets the symbol.
        /// </summary>
        public required string Symbol { get; init; }
        /// <summary>
        /// Gets or sets the base asset.
        /// </summary>
        public required string BaseAsset { get; init; }
        /// <summary>
        /// Gets or sets the quote asset.
        /// </summary>
        public required string QuoteAsset { get; init; }
        /// <summary>
        /// Gets or sets the quantity.
        /// </summary>
        public required decimal Quantity { get; init; }
        /// <summary>
        /// Gets or sets the limit price.
        /// </summary>
        public decimal? LimitPrice { get; init; }
        /// <summary>
        /// Gets or sets the quote notional.
        /// </summary>
        public decimal? QuoteNotional { get; init; }
        /// <summary>
        /// Gets or sets the reason.
        /// </summary>
        public string? Reason { get; init; }
    }
}
