using Microsoft.Extensions.Logging;

namespace Cryoptix.Strategy.Execution
{
    public sealed class OrderExecutionService(
        ILogger<OrderExecutionService> logger) : IOrderExecutionService
    {
        private readonly ILogger<OrderExecutionService> _logger = logger;

        public Task<OrderExecutionResult> ExecuteAsync(
            OrderExecutionRequest request,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            cancellationToken.ThrowIfCancellationRequested();

            _logger.LogInformation(
                "Execution requested for {Symbol}. Signal:{Signal} Quantity:{Quantity} LimitPrice:{LimitPrice}",
                request.Context.Strategy.Symbol,
                request.Signal.Signal,
                request.Quantity,
                request.LimitPrice);

            // Replace this with actual exchange order placement once you have
            // a trading REST API abstraction, e.g. IExchangeTradingApi.
            return Task.FromResult(OrderExecutionResult.Skipped(
                "Order execution service is not yet connected to an exchange trading API."));
        }
    }
}
