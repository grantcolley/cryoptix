using Cryoptix.Strategy.Logging;
using Microsoft.Extensions.Logging;

namespace Cryoptix.Strategy.Order
{
    public sealed class OrderExecutionService(ILogger<OrderExecutionService> logger) : IOrderExecutionService
    {
        private readonly ILogger<OrderExecutionService> _logger = logger;

        public Task<OrderExecutionResult> ExecuteAsync(
            OrderExecutionRequest request,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            cancellationToken.ThrowIfCancellationRequested();

            LogInformation.ExecutionRequested(
                _logger,
                request.Symbol,
                request.Side,
                request.Quantity,
                request.LimitPrice,
                request.QuoteNotional);

            return Task.FromResult(OrderExecutionResult.Skipped(
                "Order execution service is not yet connected to an exchange trading API."));
        }
    }
}
