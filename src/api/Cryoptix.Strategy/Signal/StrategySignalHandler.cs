using Cryoptix.Exchange.Models;
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
        IOrderExecutionService orderExecutionService) : IStrategySignalHandler
    {
        private readonly ILogger<StrategySignalHandler> _logger = logger;
        private readonly IStrategyClock _clock = clock;
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

            TimeSpan maxOrderBookAge = TimeSpan.FromSeconds(context.Strategy.MaxOrderBookAgeSeconds);
            TimeSpan maxAccountAge = TimeSpan.FromSeconds(context.Strategy.MaxAccountAgeSeconds);

            if (nowUtc - orderBook.UpdateTime > maxOrderBookAge)
            {
                _logger.LogWarning(
                    "Cannot handle signal for {Symbol}: order book snapshot is stale. UpdateTime:{UpdateTime:u} MaxAgeSeconds:{MaxAgeSeconds}",
                    context.Strategy.Symbol,
                    orderBook.UpdateTime,
                    context.Strategy.MaxOrderBookAgeSeconds);

                return;
            }

            if (nowUtc - account.Time > maxAccountAge)
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

            SymbolParts symbolParts = ParseSymbol(context.Strategy.Symbol);

            OrderExecutionRequest? request = BuildExecutionRequest(
                context,
                signal,
                orderBook,
                account,
                symbolParts);

            if (request == null)
            {
                return;
            }

            OrderExecutionResult result = await _orderExecutionService.ExecuteAsync(
                request,
                cancellationToken);

            if (result.Success)
            {
                _logger.LogInformation(
                    "Execution submitted for {Symbol}. Signal:{Signal} ExchangeOrderId:{ExchangeOrderId} Quantity:{Quantity} Price:{Price}",
                    context.Strategy.Symbol,
                    signal.Signal,
                    result.ExchangeOrderId,
                    result.SubmittedQuantity,
                    result.SubmittedPrice);
            }
            else
            {
                _logger.LogWarning(
                    "Execution skipped for {Symbol}. Signal:{Signal}. Reason:{Reason}",
                    context.Strategy.Symbol,
                    signal.Signal,
                    result.Reason);
            }
        }

        private OrderExecutionRequest? BuildExecutionRequest(
            StrategyAnalysisContext context,
            SignalEvaluationResult signal,
            OrderBook orderBook,
            Account account,
            SymbolParts symbolParts)
        {
            decimal? quantity = null;
            decimal? price = null;

            switch (signal.Signal)
            {
                case StrategySignal.Buy:
                    {
                        decimal quoteFree = context.AccountRealtimeState.GetFreeBalance(symbolParts.QuoteAsset);

                        if (quoteFree <= 0m)
                        {
                            _logger.LogWarning(
                                "Cannot execute BUY for {Symbol}: no free {QuoteAsset} balance.",
                                context.Strategy.Symbol,
                                symbolParts.QuoteAsset);

                            return null;
                        }

                        if (orderBook.BestAsk == null || orderBook.BestAsk.Price <= 0m)
                        {
                            _logger.LogWarning(
                                "Cannot execute BUY for {Symbol}: invalid best ask.",
                                context.Strategy.Symbol);

                            return null;
                        }

                        // Example sizing rule:
                        // use 10% of available quote balance at best ask
                        decimal notionalToSpend = quoteFree * 0.10m;
                        quantity = notionalToSpend / orderBook.BestAsk.Price;
                        price = orderBook.BestAsk.Price;

                        if (quantity <= 0m)
                        {
                            _logger.LogWarning(
                                "Cannot execute BUY for {Symbol}: calculated quantity <= 0.",
                                context.Strategy.Symbol);

                            return null;
                        }

                        break;
                    }

                case StrategySignal.Sell:
                    {
                        decimal baseFree = context.AccountRealtimeState.GetFreeBalance(symbolParts.BaseAsset);

                        if (baseFree <= 0m)
                        {
                            _logger.LogWarning(
                                "Cannot execute SELL for {Symbol}: no free {BaseAsset} balance.",
                                context.Strategy.Symbol,
                                symbolParts.BaseAsset);

                            return null;
                        }

                        if (orderBook.BestBid == null || orderBook.BestBid.Price <= 0m)
                        {
                            _logger.LogWarning(
                                "Cannot execute SELL for {Symbol}: invalid best bid.",
                                context.Strategy.Symbol);

                            return null;
                        }

                        // Example sizing rule:
                        // sell 10% of available base balance
                        quantity = baseFree * 0.10m;
                        price = orderBook.BestBid.Price;

                        if (quantity <= 0m)
                        {
                            _logger.LogWarning(
                                "Cannot execute SELL for {Symbol}: calculated quantity <= 0.",
                                context.Strategy.Symbol);

                            return null;
                        }

                        break;
                    }

                default:
                    {
                        _logger.LogInformation(
                            "Signal {Signal} for {Symbol} is currently not mapped to execution.",
                            signal.Signal,
                            context.Strategy.Symbol);

                        return null;
                    }
            }

            return new OrderExecutionRequest
            {
                Context = context,
                Signal = signal,
                OrderBook = orderBook,
                Account = account,
                BaseAsset = symbolParts.BaseAsset,
                QuoteAsset = symbolParts.QuoteAsset,
                Quantity = quantity,
                LimitPrice = price
            };
        }

        private static SymbolParts ParseSymbol(string? symbol)
        {
            if (string.IsNullOrWhiteSpace(symbol))
                throw new InvalidOperationException("Strategy symbol is required.");

            string normalized = symbol.Trim().ToUpperInvariant();

            // Minimal pragmatic parser. Extend as needed for your exchange universe.
            string[] knownQuoteAssets = ["USDT", "FDUSD", "USDC", "BTC", "ETH", "BNB", "EUR", "TRY"];

            foreach (string quote in knownQuoteAssets.OrderByDescending(x => x.Length))
            {
                if (normalized.EndsWith(quote, StringComparison.Ordinal))
                {
                    string baseAsset = normalized[..^quote.Length];
                    if (!string.IsNullOrWhiteSpace(baseAsset))
                    {
                        return new SymbolParts(baseAsset, quote);
                    }
                }
            }

            throw new InvalidOperationException($"Unable to parse base/quote assets from symbol '{symbol}'.");
        }

        private readonly struct SymbolParts
        {
            public SymbolParts(string baseAsset, string quoteAsset)
            {
                BaseAsset = baseAsset;
                QuoteAsset = quoteAsset;
            }

            public string BaseAsset { get; }
            public string QuoteAsset { get; }
        }
    }
}
