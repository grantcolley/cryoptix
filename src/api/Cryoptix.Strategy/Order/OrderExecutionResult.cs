namespace Cryoptix.Strategy.Order
{
    /// <summary>
    /// Represents the order execution result.
    /// </summary>
    public sealed class OrderExecutionResult
    {
        /// <summary>
        /// Gets or sets the success.
        /// </summary>
        public required bool Success { get; init; }
        /// <summary>
        /// Gets or sets the reason.
        /// </summary>
        public string? Reason { get; init; }
        /// <summary>
        /// Gets or sets the exchange order id.
        /// </summary>
        public string? ExchangeOrderId { get; init; }
        /// <summary>
        /// Gets or sets the submitted quantity.
        /// </summary>
        public decimal? SubmittedQuantity { get; init; }
        /// <summary>
        /// Gets or sets the submitted price.
        /// </summary>
        public decimal? SubmittedPrice { get; init; }

        /// <summary>
        /// Executes the skipped operation.
        /// </summary>
        /// <param name="reason">The reason value.</param>
        /// <returns>The skipped result.</returns>
        public static OrderExecutionResult Skipped(string reason) =>
            new()
            {
                Success = false,
                Reason = reason
            };

        /// <summary>
        /// Executes the submitted operation.
        /// </summary>
        /// <param name="exchangeOrderId">The exchange order id value.</param>
        /// <param name="quantity">The quantity value.</param>
        /// <param name="price">The price value.</param>
        /// <param name="reason">The reason value.</param>
        /// <returns>The submitted result.</returns>
        public static OrderExecutionResult Submitted(
            string? exchangeOrderId,
            decimal? quantity,
            decimal? price,
            string? reason = null) =>
            new()
            {
                Success = true,
                ExchangeOrderId = exchangeOrderId,
                SubmittedQuantity = quantity,
                SubmittedPrice = price,
                Reason = reason
            };
    }
}
