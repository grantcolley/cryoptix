using Cryoptix.Market.Data;
using Cryoptix.Strategy.Analysis;
using Cryoptix.Strategy.Clock;
using Cryoptix.Strategy.Engine;
using Cryoptix.Strategy.Order;
using Microsoft.Extensions.Logging;

namespace Cryoptix.Strategy.Signal
{
    public sealed class StrategySignalHandler(
        ILogger<StrategySignalHandler> logger,
        IStrategyClock clock,
        IOrderSizingService orderSizingService,
        IOrderExecutionService orderExecutionService) : IStrategySignalHandler
    {
        private readonly ILogger<StrategySignalHandler> _logger = logger;
        private readonly IStrategyClock _clock = clock;
        private readonly IOrderSizingService _orderSizingService = orderSizingService;
        private readonly IOrderExecutionService _orderExecutionService = orderExecutionService;

        public async Task HandleAsync(
            StrategyAnalysisContext context,
            SignalEvaluationResult signal,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(context);
            ArgumentNullException.ThrowIfNull(signal);

            cancellationToken.ThrowIfCancellationRequested();

            if (signal.Signal == StrategySignal.None)
                return;

            if (!context.OrderBookRealtimeState.TryGet(out OrderBook? orderBook) || orderBook == null)
            {
                _logger.LogWarning(
                    "Cannot handle signal for {Symbol}: no realtime order book snapshot available.",
                    context.Strategy.Symbol);
                return;
            }

            if (!context.AccountRealtimeState.TryGet(out Account? account) || account == null)
            {
                _logger.LogWarning(
                    "Cannot handle signal for {Symbol}: no realtime account snapshot available.",
                    context.Strategy.Symbol);
                return;
            }

            DateTime nowUtc = _clock.UtcNow;

            if (nowUtc - orderBook.UpdateTime > TimeSpan.FromSeconds(context.Strategy.MaxOrderBookAgeSeconds))
            {
                _logger.LogWarning(
                    "Cannot handle signal for {Symbol}: order book snapshot is stale. UpdateTime:{UpdateTime:u} MaxAgeSeconds:{MaxAgeSeconds}",
                    context.Strategy.Symbol,
                    orderBook.UpdateTime,
                    context.Strategy.MaxOrderBookAgeSeconds);
                return;
            }

            if (nowUtc - account.Time > TimeSpan.FromSeconds(context.Strategy.MaxAccountAgeSeconds))
            {
                _logger.LogWarning(
                    "Cannot handle signal for {Symbol}: account snapshot is stale. AccountTime:{AccountTime:u} MaxAgeSeconds:{MaxAgeSeconds}",
                    context.Strategy.Symbol,
                    account.Time,
                    context.Strategy.MaxAccountAgeSeconds);
                return;
            }

            if (orderBook.BestBid == null || orderBook.BestAsk == null)
            {
                _logger.LogWarning(
                    "Cannot handle signal for {Symbol}: order book best bid/ask unavailable.",
                    context.Strategy.Symbol);
                return;
            }

            OrderSizingResult? sizingResult = _orderSizingService.Size(
                context,
                signal,
                orderBook,
                account);

            if (sizingResult == null)
            {
                _logger.LogWarning(
                    "Execution skipped for {Symbol}. Signal:{Signal}. Reason:Sizing returned null.",
                    context.Strategy.Symbol,
                    signal.Signal);
                return;
            }

            if (sizingResult.Quantity <= 0m)
            {
                _logger.LogWarning(
                    "Execution skipped for {Symbol}. Signal:{Signal}. Reason:Calculated quantity <= 0.",
                    context.Strategy.Symbol,
                    signal.Signal);
                return;
            }

            OrderExecutionRequest request = new()
            {
                Context = context,
                Signal = signal,
                OrderBook = orderBook,
                Account = account,
                Side = sizingResult.Side,
                Symbol = context.Strategy.Symbol!,
                BaseAsset = sizingResult.BaseAsset,
                QuoteAsset = sizingResult.QuoteAsset,
                Quantity = sizingResult.Quantity,
                LimitPrice = sizingResult.LimitPrice,
                QuoteNotional = sizingResult.QuoteNotional,
                Reason = sizingResult.Reason
            };

            _logger.LogInformation(
                "Signal approved for execution for {Symbol} [{StrategyType}]: {Signal}. Side:{Side} Quantity:{Quantity} LimitPrice:{LimitPrice} QuoteNotional:{QuoteNotional} Reason:{Reason}",
                context.Strategy.Symbol,
                context.Strategy.StrategyProcessorType,
                signal.Signal,
                sizingResult.Side,
                sizingResult.Quantity,
                sizingResult.LimitPrice,
                sizingResult.QuoteNotional,
                sizingResult.Reason);

            OrderExecutionResult executionResult = await _orderExecutionService.ExecuteAsync(
                request,
                cancellationToken);

            if (executionResult.Success)
            {
                _logger.LogInformation(
                    "Execution submitted for {Symbol}. Side:{Side} ExchangeOrderId:{ExchangeOrderId} Quantity:{Quantity} Price:{Price}",
                    request.Symbol,
                    request.Side,
                    executionResult.ExchangeOrderId,
                    executionResult.SubmittedQuantity,
                    executionResult.SubmittedPrice);
            }
            else
            {
                _logger.LogWarning(
                    "Execution skipped for {Symbol}. Side:{Side}. Reason:{Reason}",
                    request.Symbol,
                    request.Side,
                    executionResult.Reason);
            }
        }
    }
}
