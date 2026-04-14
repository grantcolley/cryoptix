namespace Cryoptix.Strategy.Execution
{
    public sealed class OrderExecutionResult
    {
        public required bool Success { get; init; }
        public string? Reason { get; init; }
        public string? ExchangeOrderId { get; init; }
        public decimal? SubmittedQuantity { get; init; }
        public decimal? SubmittedPrice { get; init; }

        public static OrderExecutionResult Skipped(string reason) =>
            new()
            {
                Success = false,
                Reason = reason
            };

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
