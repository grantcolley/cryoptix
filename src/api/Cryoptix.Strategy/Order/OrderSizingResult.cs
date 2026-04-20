using Cryoptix.Market.Models;

namespace Cryoptix.Strategy.Order
{
    public sealed class OrderSizingResult
    {
        public required OrderSide Side { get; init; }
        public required string BaseAsset { get; init; }
        public required string QuoteAsset { get; init; }
        public required decimal Quantity { get; init; }

        /// <summary>
        /// Null means the executor may use market execution or choose the price itself.
        /// Non-null means the sizing service wants a limit-style submission price.
        /// </summary>
        public decimal? LimitPrice { get; init; }

        /// <summary>
        /// Optional notional value in quote asset terms.
        /// Useful for logging, validation, and exchange min-notional checks.
        /// </summary>
        public decimal? QuoteNotional { get; init; }

        /// <summary>
        /// Human-readable explanation for logs/debugging.
        /// </summary>
        public string? Reason { get; init; }
    }
}
