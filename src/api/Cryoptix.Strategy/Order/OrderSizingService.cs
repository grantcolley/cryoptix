using Cryoptix.Market.Data;
using Cryoptix.Market.Strategy;
using Cryoptix.Strategy.Analysis;
using Cryoptix.Strategy.Engine;
using Cryoptix.Strategy.Logging;
using Microsoft.Extensions.Logging;

namespace Cryoptix.Strategy.Order
{
    /// <summary>
    /// Represents the order sizing service.
    /// </summary>
    public sealed class OrderSizingService(ILogger<OrderSizingService> logger) : IOrderSizingService
    {
        private readonly ILogger<OrderSizingService> _logger = logger;

        /// <summary>
        /// Executes the size operation.
        /// </summary>
        /// <param name="context">The context value.</param>
        /// <param name="signalEvaluationResult">The signal evaluation result value.</param>
        /// <param name="orderBook">The order book value.</param>
        /// <param name="account">The account value.</param>
        /// <returns>The size result.</returns>
        public OrderSizingResult? Size(
            StrategyAnalysisContext context,
            SignalEvaluationResult signalEvaluationResult,
            OrderBook orderBook,
            Account account)
        {
            ArgumentNullException.ThrowIfNull(context);
            ArgumentNullException.ThrowIfNull(signalEvaluationResult);
            ArgumentNullException.ThrowIfNull(orderBook);
            ArgumentNullException.ThrowIfNull(account);

            SymbolParts parts = ParseSymbol(context.Strategy.Symbol);

            switch (signalEvaluationResult.Signal.SignalType)
            {
                case SignalType.Buy:
                    {
                        decimal quoteFree = context.AccountRealtimeState.GetFreeBalance(parts.QuoteAsset);
                        if (quoteFree <= 0m)
                        {
                            _logger.LogWarning(
                                "Sizing failed for {Symbol}: no free {QuoteAsset} balance.",
                                context.Strategy.Symbol,
                                parts.QuoteAsset);

                            return null;
                        }

                        decimal bestAsk = orderBook.BestAsk?.Price ?? 0m;
                        if (bestAsk <= 0m)
                        {
                            _logger.LogWarning(
                                "Sizing failed for {Symbol}: invalid best ask.",
                                context.Strategy.Symbol);

                            return null;
                        }

                        decimal quoteNotional = quoteFree * 0.10m;
                        decimal quantity = quoteNotional / bestAsk;

                        if (quantity <= 0m)
                        {
                            _logger.LogWarning(
                                "Sizing failed for {Symbol}: calculated buy quantity <= 0.",
                                context.Strategy.Symbol);

                            return null;
                        }

                        return new OrderSizingResult
                        {
                            Side = OrderSide.Buy,
                            BaseAsset = parts.BaseAsset,
                            QuoteAsset = parts.QuoteAsset,
                            Quantity = quantity,
                            LimitPrice = bestAsk,
                            QuoteNotional = quoteNotional,
                            Reason = "10% quote balance sizing"
                        };
                    }

                case SignalType.Sell:
                    {
                        decimal baseFree = context.AccountRealtimeState.GetFreeBalance(parts.BaseAsset);
                        if (baseFree <= 0m)
                        {
                            _logger.LogWarning(
                                "Sizing failed for {Symbol}: no free {BaseAsset} balance.",
                                context.Strategy.Symbol,
                                parts.BaseAsset);

                            return null;
                        }

                        decimal bestBid = orderBook.BestBid?.Price ?? 0m;
                        if (bestBid <= 0m)
                        {
                            _logger.LogWarning(
                                "Sizing failed for {Symbol}: invalid best bid.",
                                context.Strategy.Symbol);

                            return null;
                        }

                        decimal quantity = baseFree * 0.10m;
                        decimal quoteNotional = quantity * bestBid;

                        if (quantity <= 0m)
                        {
                            _logger.LogWarning(
                                "Sizing failed for {Symbol}: calculated sell quantity <= 0.",
                                context.Strategy.Symbol);

                            return null;
                        }

                        return new OrderSizingResult
                        {
                            Side = OrderSide.Sell,
                            BaseAsset = parts.BaseAsset,
                            QuoteAsset = parts.QuoteAsset,
                            Quantity = quantity,
                            LimitPrice = bestBid,
                            QuoteNotional = quoteNotional,
                            Reason = "10% base balance sizing"
                        };
                    }

                default:
                    LogDebug.SizingSkipped(
                        _logger,
                        context.Strategy.Symbol!,
                        signalEvaluationResult.Signal.TimestampUtc,
                        signalEvaluationResult.Signal.SignalType,
                        signalEvaluationResult.Signal.Reason);

                    return null;
            }
        }

        private static SymbolParts ParseSymbol(string? symbol)
        {
            if (string.IsNullOrWhiteSpace(symbol))
                throw new InvalidOperationException("Strategy symbol is required.");

            string normalized = symbol.Trim().ToUpperInvariant();

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

        private readonly struct SymbolParts(string baseAsset, string quoteAsset)
        {
            /// <summary>
            /// Gets the base asset.
            /// </summary>
            public string BaseAsset { get; } = baseAsset;
            /// <summary>
            /// Gets the quote asset.
            /// </summary>
            public string QuoteAsset { get; } = quoteAsset;
        }
    }
}
