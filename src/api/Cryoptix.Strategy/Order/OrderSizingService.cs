using Cryoptix.Market.Data;
using Cryoptix.Market.Strategy;
using Cryoptix.Strategy.Analysis;
using Cryoptix.Strategy.Engine;
using Microsoft.Extensions.Logging;

namespace Cryoptix.Strategy.Order
{
    public sealed class OrderSizingService(ILogger<OrderSizingService> logger) : IOrderSizingService
    {
        private readonly ILogger<OrderSizingService> _logger = logger;

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
                    _logger.LogDebug(
                        "Sizing skipped for {Symbol}: signal {Signal} is not executable.",
                        context.Strategy.Symbol,
                        signalEvaluationResult.Signal);

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
