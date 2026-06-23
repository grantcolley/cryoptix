using Cryoptix.Market.Data;

namespace Cryoptix.Strategy.Order
{
    /// <summary>
    /// Represents the order sizing result.
    /// </summary>
    public sealed class OrderSizingResult
    {
        /// <summary>
        /// Gets or sets the side.
        /// </summary>
        public required OrderSide Side { get; init; }
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
        /// <summary>
        /// Null means the executor may use market execution or choose the price itself.
        /// Non-null means the sizing service wants a limit-style submission price.
        /// </summary>
        public decimal? LimitPrice { get; init; }

        /// <summary>
        /// Gets or sets the quote notional.
        /// </summary>
        /// <summary>
        /// Optional notional value in quote asset terms.
        /// Useful for logging, validation, and exchange min-notional checks.
        /// </summary>
        public decimal? QuoteNotional { get; init; }

        /// <summary>
        /// Gets or sets the reason.
        /// </summary>
        /// <summary>
        /// Human-readable explanation for logs/debugging.
        /// </summary>
        public string? Reason { get; init; }
    }
}
