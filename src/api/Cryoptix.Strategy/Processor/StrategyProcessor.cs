using Cryoptix.Exchange.Api;
using Cryoptix.Exchange.Models;
using Cryoptix.Strategy.Agent;
using Microsoft.Extensions.Logging;
using System.Threading.Channels;

namespace Cryoptix.Strategy.Processor
{
    public class StrategyProcessor(ILogger<StrategyProcessor> logger) : IStrategyProcessor
    {
        public readonly StrategyProcessorType StrategyProcessorType = StrategyProcessorType.TradingFlow;

        private readonly ILogger<StrategyProcessor> _logger = logger;

        public async Task ExecuteAsync(StrategyAgentSession strategyAgentSession, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(strategyAgentSession);
            ArgumentNullException.ThrowIfNull(strategyAgentSession.ExchangeApi);
            ArgumentNullException.ThrowIfNull(strategyAgentSession.ExchangeApi.RestApi);
            ArgumentNullException.ThrowIfNull(strategyAgentSession.ExchangeApi.SubscriptionsApi);

            if (strategyAgentSession.GetStrategy is null)
                throw new ArgumentNullException($"{nameof(strategyAgentSession)}.GetStrategy");
            if (strategyAgentSession.WaitForStrategyUpdateAsync is null)
                throw new ArgumentNullException($"{nameof(strategyAgentSession)}.WaitForStrategyUpdateAsync");

            Runtime.Strategy? initialStrategy = strategyAgentSession.GetStrategy();
            ValidateInitialStrategy(initialStrategy);

            StrategyProcessorSession strategyProcessorSession = new()
            {
                ExchangeApi = strategyAgentSession.ExchangeApi,
                Strategy = initialStrategy!,
                Cache = new MarketDataCache(
                    maxTradesPerSymbol: 10_000,
                    maxKlinesPerSeries: 5_000)
            };

            Channel<MarketEvent> marketEventChannel = Channel.CreateUnbounded<MarketEvent>(
                new UnboundedChannelOptions
                {
                    SingleReader = true,
                    SingleWriter = false,
                    AllowSynchronousContinuations = false
                });

            StrategyProcessorSubscriptions? strategyProcessorSubscriptions = null;
            Task processingTask = Task.CompletedTask;

            try
            {
                await SeedStrategyAsync(
                    strategy: strategyProcessorSession.Strategy,
                    restApi: strategyProcessorSession.ExchangeApi.RestApi!,
                    writer: marketEventChannel.Writer,
                    cancellationToken: cancellationToken);

                strategyProcessorSubscriptions = await StartStrategySubscriptionsAsync(
                    strategy: strategyProcessorSession.Strategy,
                    subscriptionsApi: strategyProcessorSession.ExchangeApi.SubscriptionsApi!,
                    writer: marketEventChannel.Writer,
                    cancellationToken: cancellationToken);

                processingTask = ProcessMarketEventsAsync(
                    strategyProcessorSession: strategyProcessorSession,
                    reader: marketEventChannel.Reader,
                    cancellationToken: cancellationToken);

                while (!cancellationToken.IsCancellationRequested)
                {
                    Task strategyUpdateTask = strategyAgentSession.WaitForStrategyUpdateAsync(cancellationToken);
                    Task subscriptionCompletionTask = strategyProcessorSubscriptions.Completion;

                    Task completed = await Task.WhenAny(
                        strategyUpdateTask,
                        subscriptionCompletionTask,
                        processingTask);

                    if (completed == processingTask)
                    {
                        await processingTask;
                        break;
                    }

                    if (completed == subscriptionCompletionTask)
                    {
                        await subscriptionCompletionTask;
                        break;
                    }

                    Runtime.Strategy? updatedStrategy = strategyAgentSession.GetStrategy();
                    if (updatedStrategy == null)
                    {
                        _logger.LogWarning("Received null strategy update; ignoring.");
                        continue;
                    }

                    ValidateCompatibleStrategyUpdate(strategyProcessorSession.Strategy, updatedStrategy);

                    strategyProcessorSession.Strategy = updatedStrategy;

                    _logger.LogInformation(
                        "Applied strategy update for {Symbol}. Subscriptions unchanged.",
                        strategyProcessorSession.Strategy.Symbol);
                }
            }
            finally
            {
                if (strategyProcessorSubscriptions != null)
                {
                    await strategyProcessorSubscriptions.DisposeAsync();
                }

                marketEventChannel.Writer.TryComplete();

                try
                {
                    await processingTask;
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                }
            }
        }

        private static void ValidateInitialStrategy(Runtime.Strategy? strategy)
        {
            if (strategy == null)
                throw new InvalidOperationException("Strategy is required.");

            if (string.IsNullOrWhiteSpace(strategy.Symbol))
                throw new InvalidOperationException("Strategy symbol is required.");

            if (strategy.KlineInterval == default)
                throw new InvalidOperationException("Strategy kline interval is required.");
        }

        private static void ValidateCompatibleStrategyUpdate(Runtime.Strategy current, Runtime.Strategy updated)
        {
            if (!string.Equals(current.Symbol, updated.Symbol, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Strategy update cannot change Symbol while processor is running.");
            }

            if (current.KlineInterval != updated.KlineInterval)
            {
                throw new InvalidOperationException("Strategy update cannot change Interval while processor is running.");
            }
        }

        private async Task SeedStrategyAsync(
            Runtime.Strategy strategy,
            IExchangeRestApi restApi,
            ChannelWriter<MarketEvent> writer,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            DateTime endTime = DateTime.UtcNow;
            DateTime startTime = GetSeedStartTime(strategy, endTime);

            _logger.LogInformation(
                "Fetching historical klines for {Symbol} {Interval} from {Start:u} to {End:u}",
                strategy.Symbol,
                strategy.KlineInterval,
                startTime,
                endTime);

            List<Kline> historicalKlines = await restApi.GetKlinesAsync(
                symbol: strategy.Symbol!,
                interval: strategy.KlineInterval,
                startTime: startTime,
                endTime: endTime,
                limit: null,
                cancellationToken: cancellationToken);

            foreach (Kline kline in historicalKlines.OrderBy(k => k.OpenTime))
            {
                await writer.WriteAsync(new KlineMarketEvent(kline, MarketEventSource.Seed), cancellationToken);
            }

            _logger.LogInformation(
                "Seeded {Count} klines for {Symbol} {Interval}",
                historicalKlines.Count,
                strategy.Symbol,
                strategy.KlineInterval);
        }

        private async Task<StrategyProcessorSubscriptions> StartStrategySubscriptionsAsync(
            Runtime.Strategy strategy,
            IExchangeSubscriptionApi subscriptionsApi,
            ChannelWriter<MarketEvent> writer,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (string.IsNullOrWhiteSpace(strategy.Symbol))
                throw new InvalidOperationException("Strategy symbol is required.");

            CancellationTokenSource sessionCancellationTokenSource =
                CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            IAsyncDisposable? klineSubscription = null;
            IAsyncDisposable? tradeSubscription = null;

            try
            {
                klineSubscription = await subscriptionsApi.SubscribeToKlineUpdatesAsync(
                    symbol: strategy.Symbol,
                    interval: strategy.KlineInterval,
                    onCallback: args =>
                    {
                        if(args.Klines == null || !args.Klines.Any())
                        {
                            _logger.LogWarning(
                                "Received kline update with no klines for {Symbol} {Interval}",
                                strategy.Symbol,
                                strategy.KlineInterval);
                            return;
                        }

                        foreach (Kline kline in args.Klines)
                        {
                            if (!writer.TryWrite(new KlineMarketEvent(kline, MarketEventSource.Live)))
                            {
                                _logger.LogWarning(
                                    "Failed to enqueue live kline for {Symbol} {Interval}; channel closed.",
                                    kline.Symbol,
                                    kline.Interval);
                            }
                        }
                    },
                    onError: ex =>
                    {
                        _logger.LogError(ex,
                            "Kline subscription error for {Symbol} {Interval}",
                            strategy.Symbol,
                            strategy.KlineInterval);
                    },
                    cancellationToken: sessionCancellationTokenSource.Token);

                tradeSubscription = await subscriptionsApi.SubscribeToTradesAsync(
                    symbol: strategy.Symbol,
                    onCallback: args =>
                    {
                        if(args.Trades == null || !args.Trades.Any())
                        {
                            _logger.LogWarning(
                                "Received trade update with no trades for {Symbol}",
                                strategy.Symbol);
                            return;
                        }

                        foreach (Trade trade in args.Trades)
                        {
                            if (!writer.TryWrite(new TradeMarketEvent(trade)))
                            {
                                _logger.LogWarning(
                                    "Failed to enqueue live trade for {Symbol}; channel closed.",
                                    trade.Symbol);
                            }
                        }
                    },
                    onError: ex =>
                    {
                        _logger.LogError(ex,
                            "Trade subscription error for {Symbol}",
                            strategy.Symbol);
                    },
                    cancellationToken: sessionCancellationTokenSource.Token);

                CompositeAsyncDisposable compositeHandle = new(klineSubscription, tradeSubscription);

                Task completionTask = WaitUntilCancelledCleanlyAsync(sessionCancellationTokenSource.Token);

                _logger.LogInformation(
                    "Started subscriptions for {Symbol} {Interval}",
                    strategy.Symbol,
                    strategy.KlineInterval);

                return new StrategyProcessorSubscriptions(compositeHandle, sessionCancellationTokenSource, completionTask);
            }
            catch
            {
                if (tradeSubscription != null)
                {
                    try { await tradeSubscription.DisposeAsync(); } catch { }
                }

                if (klineSubscription != null)
                {
                    try { await klineSubscription.DisposeAsync(); } catch { }
                }

                sessionCancellationTokenSource.Dispose();
                throw;
            }
        }

        private async Task ProcessMarketEventsAsync(
            StrategyProcessorSession strategyProcessorSession,
            ChannelReader<MarketEvent> reader,
            CancellationToken cancellationToken)
        {
            await foreach (MarketEvent marketEvent in reader.ReadAllAsync(cancellationToken))
            {
                switch (marketEvent)
                {
                    case KlineMarketEvent klineEvent:
                        await ProcessKlineEventAsync(strategyProcessorSession, klineEvent, cancellationToken);
                        break;

                    case TradeMarketEvent tradeEvent:
                        await ProcessTradeEventAsync(strategyProcessorSession, tradeEvent, cancellationToken);
                        break;

                    default:
                        _logger.LogWarning("Unknown market event type {EventType}", marketEvent.GetType().Name);
                        break;
                }
            }
        }

        private Task ProcessKlineEventAsync(
            StrategyProcessorSession strategyProcessorSession,
            KlineMarketEvent marketEvent,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            Runtime.Strategy strategy = strategyProcessorSession.Strategy;
            Kline incomingKline = marketEvent.Kline;

            KlineUpsertResult upsertResult = strategyProcessorSession.Cache.UpsertKline(incomingKline);

            IReadOnlyList<Kline> cachedKlines = strategyProcessorSession.Cache.GetKlines(strategy.Symbol!, strategy.KlineInterval);
            Kline? latestKline = strategyProcessorSession.Cache.GetLatestKline(strategy.Symbol!, strategy.KlineInterval);

            _logger.LogInformation(
                "Processed {Source} kline for {Symbol} {Interval} OpenTime:{OpenTime:u} Final:{Final} " +
                "Inserted:{Inserted} Updated:{Updated} CacheCount:{CacheCount} LatestOpenTime:{LatestOpenTime}",
                marketEvent.Source,
                incomingKline.Symbol,
                incomingKline.Interval,
                incomingKline.OpenTime,
                incomingKline.Final,
                upsertResult.Inserted,
                upsertResult.Updated,
                cachedKlines.Count,
                latestKline?.OpenTime);

            // Do all indicator calculations here, outside websocket callbacks.
            // Example:
            //
            // _indicatorEngine.Update(strategy, cachedKlines);
            // _signalEngine.Evaluate(strategy, cachedKlines, session.Cache.GetTrades(strategy.Symbol));
            //
            // Because seeded/live klines both pass through the same cache and same path,
            // dedupe/upsert behavior stays consistent.

            return Task.CompletedTask;
        }

        private Task ProcessTradeEventAsync(
            StrategyProcessorSession strategyProcessorSession,
            TradeMarketEvent marketEvent,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            Runtime.Strategy strategy = strategyProcessorSession.Strategy;
            Trade incomingTrade = marketEvent.Trade;

            bool added = strategyProcessorSession.Cache.AddTrade(incomingTrade);
            if (!added)
            {
                _logger.LogDebug(
                    "Ignored duplicate trade {TradeId} for {Symbol}",
                    incomingTrade.Id,
                    incomingTrade.Symbol);

                return Task.CompletedTask;
            }

            IReadOnlyList<Trade> cachedTrades = strategyProcessorSession.Cache.GetTrades(strategy.Symbol!);

            _logger.LogInformation(
                "Processed trade for {Symbol} TradeId:{TradeId} Time:{Time:u} Price:{Price} QuoteQty:{QuoteQty} TradeCacheCount:{TradeCacheCount}",
                incomingTrade.Symbol,
                incomingTrade.Id,
                incomingTrade.Time,
                incomingTrade.Price,
                incomingTrade.QuoteQuantity,
                cachedTrades.Count);

            // Trade-driven indicator/signal work goes here.
            // Example:
            //
            // _signalEngine.OnTrade(strategy, cachedTrades, session.Cache.GetKlines(strategy.Symbol, strategy.Interval));

            return Task.CompletedTask;
        }

        private static DateTime GetSeedStartTime(Runtime.Strategy strategy, DateTime endTimeUtc)
        {
            // Replace with strategy-specific lookback logic.
            // This is just a sensible default.
            return endTimeUtc.AddDays(-2);
        }

        private static async Task WaitUntilCancelledCleanlyAsync(CancellationToken cancellationToken)
        {
            try
            {
                await Task.Delay(Timeout.Infinite, cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
            }
        }

        private sealed class StrategyProcessorSession
        {
            public required ExchangeApi ExchangeApi { get; init; }
            public required Runtime.Strategy Strategy { get; set; }
            public required MarketDataCache Cache { get; init; }
        }

        private enum MarketEventSource
        {
            Seed,
            Live
        }

        private abstract record MarketEvent;

        private sealed record KlineMarketEvent(Kline Kline, MarketEventSource Source) : MarketEvent;

        private sealed record TradeMarketEvent(Trade Trade) : MarketEvent;

        private sealed class CompositeAsyncDisposable(params IAsyncDisposable[] inner) : IAsyncDisposable
        {
            private readonly IAsyncDisposable[] _inner = inner;
            private int _disposed;

            public async ValueTask DisposeAsync()
            {
                if (Interlocked.Exchange(ref _disposed, 1) != 0)
                    return;

                List<Exception>? exceptions = null;

                for (int i = _inner.Length - 1; i >= 0; i--)
                {
                    try
                    {
                        await _inner[i].DisposeAsync();
                    }
                    catch (Exception ex)
                    {
                        exceptions ??= [];
                        exceptions.Add(ex);
                    }
                }

                if (exceptions is { Count: > 0 })
                {
                    throw new AggregateException(exceptions);
                }
            }
        }

        private sealed class MarketDataCache
        {
            private readonly int _maxTradesPerSymbol;
            private readonly int _maxKlinesPerSeries;

            // (symbol, interval) -> openTime -> kline
            private readonly Dictionary<(string Symbol, KlineInterval Interval), SortedDictionary<DateTime, Kline>> _klines = [];

            // symbol -> rolling trades in arrival order
            private readonly Dictionary<string, LinkedList<Trade>> _trades = [];

            // symbol -> trade ids for dedupe
            private readonly Dictionary<string, HashSet<long>> _tradeIds = [];

            public MarketDataCache(int maxTradesPerSymbol, int maxKlinesPerSeries)
            {
                if (maxTradesPerSymbol <= 0)
                    throw new ArgumentOutOfRangeException(nameof(maxTradesPerSymbol));

                if (maxKlinesPerSeries <= 0)
                    throw new ArgumentOutOfRangeException(nameof(maxKlinesPerSeries));

                _maxTradesPerSymbol = maxTradesPerSymbol;
                _maxKlinesPerSeries = maxKlinesPerSeries;
            }

            public KlineUpsertResult UpsertKline(Kline kline)
            {
                ArgumentNullException.ThrowIfNull(kline);

                string symbol = NormalizeSymbol(kline.Symbol);
                var seriesKey = (symbol, kline.Interval);

                if (!_klines.TryGetValue(seriesKey, out var series))
                {
                    series = [];
                    _klines[seriesKey] = series;
                }

                bool existed = series.TryGetValue(kline.OpenTime, out Kline? existing);

                // Upsert using (symbol, interval, openTime) identity.
                // The latest version wins, which handles:
                // - seeded historical klines
                // - repeated live partial updates
                // - final live candle replacing partial candle
                series[kline.OpenTime] = kline;

                TrimKlineSeries(series);

                if (!existed)
                {
                    return new KlineUpsertResult(
                        Inserted: true,
                        Updated: false,
                        Previous: null,
                        Current: kline);
                }

                bool materiallyChanged = !AreEquivalent(existing!, kline);

                return new KlineUpsertResult(
                    Inserted: false,
                    Updated: materiallyChanged,
                    Previous: existing,
                    Current: kline);
            }

            public bool AddTrade(Trade trade)
            {
                ArgumentNullException.ThrowIfNull(trade);

                string symbol = NormalizeSymbol(trade.Symbol!);

                if (!_trades.TryGetValue(symbol, out var trades))
                {
                    trades = [];
                    _trades[symbol] = trades;
                }

                if (!_tradeIds.TryGetValue(symbol, out var tradeIds))
                {
                    tradeIds = [];
                    _tradeIds[symbol] = tradeIds;
                }

                if (!tradeIds.Add(trade.Id))
                {
                    return false;
                }

                trades.AddLast(trade);

                while (trades.Count > _maxTradesPerSymbol)
                {
                    LinkedListNode<Trade>? oldest = trades.First;
                    if (oldest == null)
                        break;

                    trades.RemoveFirst();
                    tradeIds.Remove(oldest.Value.Id);
                }

                return true;
            }

            public IReadOnlyList<Kline> GetKlines(string symbol, KlineInterval interval)
            {
                var key = (NormalizeSymbol(symbol), interval);

                if (!_klines.TryGetValue(key, out var series))
                    return [];

                return [.. series.Values];
            }

            public Kline? GetLatestKline(string symbol, KlineInterval interval)
            {
                var key = (NormalizeSymbol(symbol), interval);

                if (!_klines.TryGetValue(key, out var series) || series.Count == 0)
                    return null;

                return series.Values.Last();
            }

            public IReadOnlyList<Trade> GetTrades(string symbol)
            {
                symbol = NormalizeSymbol(symbol);

                if (!_trades.TryGetValue(symbol, out var trades))
                    return [];

                return [.. trades];
            }

            private void TrimKlineSeries(SortedDictionary<DateTime, Kline> series)
            {
                while (series.Count > _maxKlinesPerSeries)
                {
                    DateTime oldestKey = series.First().Key;
                    series.Remove(oldestKey);
                }
            }

            private static string NormalizeSymbol(string symbol) =>
                symbol.Trim().ToUpperInvariant();

            private static bool AreEquivalent(Kline x, Kline y)
            {
                return string.Equals(x.Symbol, y.Symbol, StringComparison.OrdinalIgnoreCase)
                    && x.Interval == y.Interval
                    && x.OpenTime == y.OpenTime
                    && x.CloseTime == y.CloseTime
                    && x.Open == y.Open
                    && x.High == y.High
                    && x.Low == y.Low
                    && x.Close == y.Close
                    && x.Volume == y.Volume
                    && x.NumberOfTrades == y.NumberOfTrades
                    && x.QuoteAssetVolume == y.QuoteAssetVolume
                    && x.TakerBuyQuoteAssetVolume == y.TakerBuyQuoteAssetVolume
                    && x.TakerBuyBaseAssetVolume == y.TakerBuyBaseAssetVolume
                    && x.Final == y.Final;
            }
        }

        private readonly record struct KlineUpsertResult(
            bool Inserted,
            bool Updated,
            Kline? Previous,
            Kline Current);
    }
}
