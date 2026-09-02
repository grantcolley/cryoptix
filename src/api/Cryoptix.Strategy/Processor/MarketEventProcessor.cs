using Cryoptix.Market.Data;
using Cryoptix.Observer.Metrics;
using Cryoptix.Strategy.Agent;
using Cryoptix.Strategy.Cache;
using Cryoptix.Strategy.Channel;
using Cryoptix.Strategy.Dispatcher;
using Cryoptix.Strategy.Event;
using Cryoptix.Strategy.Logging;
using Cryoptix.Strategy.Notification;
using Cryoptix.Strategy.Seeding;
using Cryoptix.Strategy.Snapshot;
using Cryoptix.Strategy.Subscription;
using Microsoft.Extensions.Logging;
using System.Threading.Channels;

namespace Cryoptix.Strategy.Processor
{
    /// <summary>
    /// Represents the market event processor.
    /// </summary>
    public sealed class MarketEventProcessor(
        ILogger<MarketEventProcessor> logger,
        IStrategyMarketSeeder strategyMarketSeeder,
        IStrategyMarketEventSubscriber strategyMarketEventSubscriber,
        IStrategyMarketEventDispatcher strategyMarketEventDispatcher,
        IStrategyEventChannelFactory strategyEventChannelFactory,
        IMarketEventSessionAccessor marketEventSessionAccessor,
        IStrategyStatusNotifier strategyStatusNotifier,
        INotificationMetrics notificationMetrics,
        INotificationPump notificationPump) : IStrategyProcessor
    {
        /// <summary>
        /// Gets the market event processor type.
        /// </summary>
        public readonly StrategyProcessorType StrategyProcessorType = StrategyProcessorType.MarketEvent;

        private readonly ILogger<MarketEventProcessor> _logger = logger;
        private readonly IStrategyMarketSeeder _strategyMarketSeeder = strategyMarketSeeder;
        private readonly IStrategyMarketEventSubscriber _strategyMarketEventSubscriber = strategyMarketEventSubscriber;
        private readonly IStrategyMarketEventDispatcher _strategyMarketEventDispatcher = strategyMarketEventDispatcher;
        private readonly IStrategyEventChannelFactory _strategyEventChannelFactory = strategyEventChannelFactory;
        private readonly IMarketEventSessionAccessor _marketEventSessionAccessor = marketEventSessionAccessor;
        private readonly IStrategyStatusNotifier _strategyStatusNotifier = strategyStatusNotifier;
        private readonly INotificationMetrics _notificationMetrics = notificationMetrics;
        private readonly INotificationPump _notificationPump = notificationPump;

        /// <summary>
        /// Executes the execute async operation.
        /// </summary>
        /// <param name="strategyAgentSession">The strategy agent session value.</param>
        /// <param name="cancellationToken">The cancellation token value.</param>
        /// <returns>The execute async result.</returns>
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

            Strategies.Strategy? initialStrategy = strategyAgentSession.GetStrategy();
            ValidateInitialStrategy(initialStrategy);

            StrategyProcessorSession strategyProcessorSession = new()
            {
                ExchangeApi = strategyAgentSession.ExchangeApi,
                Strategy = initialStrategy!,
                Credentials = strategyAgentSession.Credentials,
                OrderBookRealtimeState = new OrderBookRealtimeState(),
                AccountRealtimeState = new AccountRealtimeState(),
                Cache = new MarketDataCache(
                    maxTradesPerSymbol: initialStrategy.CacheMaxTradesPerSymbol,
                    maxKlinesPerSeries: initialStrategy.CacheMaxKlinesPerSeries,
                    maxIndicatorsPerSeries: initialStrategy.CacheMaxIndicatorsPerSeries,
                    maxSignalsPerSeries: initialStrategy.CacheMaxSignalsPerSeries)
            };

            _marketEventSessionAccessor.SetCurrent(strategyProcessorSession);

            StrategyEventChannels channels = _strategyEventChannelFactory.Create(initialStrategy);

            StrategyMarketEventSubscriptions? strategyMarketEventSubscriptions = null;
            Task processingTask = Task.CompletedTask;
            Task notificationTask = Task.CompletedTask;

            try
            {
                await _strategyStatusNotifier.NotifyStartedAsync(
                    strategyProcessorSession.Strategy,
                    cancellationToken);

                await _strategyMarketSeeder.SeedAsync(
                    strategy: strategyProcessorSession.Strategy,
                    restApi: strategyProcessorSession.ExchangeApi.RestApi!,
                    klineWriter: channels.Klines.Writer,
                    cache: strategyProcessorSession.Cache,
                    cancellationToken: cancellationToken);

                await _strategyStatusNotifier.NotifyMarketDataSnapshotAsync(cancellationToken);

                strategyMarketEventSubscriptions = await _strategyMarketEventSubscriber.SubscribeAsync(
                    strategy: strategyProcessorSession.Strategy,
                    credentials: strategyProcessorSession.Credentials,
                    subscriptionsApi: strategyProcessorSession.ExchangeApi.SubscriptionsApi!,
                    channels: channels,
                    orderBookRealtimeState: strategyProcessorSession.OrderBookRealtimeState,
                    accountRealtimeState: strategyProcessorSession.AccountRealtimeState,
                    cancellationToken: cancellationToken);

                processingTask = ProcessMarketEventsAsync(
                    strategyProcessorSession,
                    channels,
                    cancellationToken);

                notificationTask = _notificationPump.RunAsync(
                    strategy: strategyProcessorSession.Strategy,
                    channels,
                    cancellationToken);

                while (!cancellationToken.IsCancellationRequested)
                {
                    Task strategyUpdateTask = strategyAgentSession.WaitForStrategyUpdateAsync(cancellationToken);
                    Task subscriptionCompletionTask = strategyMarketEventSubscriptions.Completion;

                    Task completed = await Task.WhenAny(
                        strategyUpdateTask,
                        subscriptionCompletionTask,
                        processingTask,
                        notificationTask);

                    if (completed == processingTask)
                    {
                        await processingTask;
                        break;
                    }

                    if (completed == notificationTask)
                    {
                        await notificationTask;
                        break;
                    }

                    if (completed == subscriptionCompletionTask)
                    {
                        await subscriptionCompletionTask;
                        break;
                    }

                    Strategies.Strategy? updatedStrategy = strategyAgentSession.GetStrategy();
                    if (updatedStrategy == null)
                    {
                        _logger.LogWarning("Received null strategy update; ignoring. (ignored)");
                        continue;
                    }

                    ValidateCompatibleStrategyUpdate(strategyProcessorSession.Strategy, updatedStrategy);

                    strategyProcessorSession.Strategy = updatedStrategy;

                    LogInformation.ApplyStrategyUpdate(_logger, strategyProcessorSession.Strategy.Symbol!);

                    await _strategyStatusNotifier.NotifyUpdatedAsync(
                        strategyProcessorSession.Strategy,
                        cancellationToken);
                }
            }
            finally
            {
                _marketEventSessionAccessor.ClearCurrent();

                if (strategyMarketEventSubscriptions != null)
                {
                    await strategyMarketEventSubscriptions.DisposeAsync();
                }

                channels.CompleteWriters();

                try
                {
                    await Task.WhenAll(processingTask, notificationTask);
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                }
            }
        }

        private async Task ProcessMarketEventsAsync(
            StrategyProcessorSession session,
            StrategyEventChannels channels,
            CancellationToken cancellationToken)
        {
            Task klineReaderTask = ReadKlineEventsAsync(
                channels.Klines.Reader,
                channels.MarketEventDispatcher.Writer,
                cancellationToken);

            Task tradeReaderTask = ReadTradeEventsAsync(
                channels.Trades.Reader,
                channels.MarketEventDispatcher.Writer,
                cancellationToken);

            Task dispatcherTask = DispatchMarketEventsAsync(
                session,
                channels,
                cancellationToken);

            await Task.WhenAll(klineReaderTask, tradeReaderTask);

            await dispatcherTask;
        }

        private static async Task ReadKlineEventsAsync(
            ChannelReader<KlineMarketEvent> reader,
            ChannelWriter<MarketEvent> writer,
            CancellationToken cancellationToken)
        {
            await foreach (KlineMarketEvent marketEvent in reader.ReadAllAsync(cancellationToken))
            {
                await writer.WriteAsync(marketEvent, cancellationToken);
            }
        }

        private static async Task ReadTradeEventsAsync(
            ChannelReader<TradeMarketEvent> reader,
            ChannelWriter<MarketEvent> writer,
            CancellationToken cancellationToken)
        {
            await foreach (TradeMarketEvent marketEvent in reader.ReadAllAsync(cancellationToken))
            {
                await writer.WriteAsync(marketEvent, cancellationToken);
            }
        }

        private async Task DispatchMarketEventsAsync(
            StrategyProcessorSession session, 
            StrategyEventChannels channels, 
            CancellationToken cancellationToken)
        {
            ChannelWriter<Kline> klineBroadcastWriter = channels.KlineBroadcasts.Writer;
            ChannelWriter<Trade> tradeBroadcastWriter = channels.TradeBroadcasts.Writer;
            ChannelReader<MarketEvent> marketEventReader = channels.MarketEventDispatcher.Reader;

            await foreach (MarketEvent marketEvent in marketEventReader.ReadAllAsync(cancellationToken))
            {
                switch (marketEvent)
                {
                    case KlineMarketEvent klineEvent:
                        if (!klineBroadcastWriter.TryWrite(klineEvent.Kline))
                        {
                            LogDebug.KlineDropped(
                                _logger,
                                klineEvent.Kline.Symbol!,
                                klineEvent.Kline.Interval);

                            _notificationMetrics.RecordBroadcastDropKline(
                                klineEvent.Kline.Symbol,
                                klineEvent.Kline.Interval);
                        }

                        break;

                    case TradeMarketEvent tradeEvent:
                        if (!tradeBroadcastWriter.TryWrite(tradeEvent.Trade))
                        {
                            LogDebug.TradeDropped(
                                _logger,
                                tradeEvent.Trade.Symbol!,
                                tradeEvent.Trade.Id);

                            _notificationMetrics.RecordBroadcastDropTrade(
                                tradeEvent.Trade.Symbol);
                        }

                        break;
                }

                await _strategyMarketEventDispatcher.DispatchAsync(
                    session,
                    marketEvent,
                    channels,
                    cancellationToken);
            }
        }

        private static void ValidateInitialStrategy(Strategies.Strategy? strategy)
        {
            if (strategy == null)
                throw new InvalidOperationException("Strategy is required.");

            if (string.IsNullOrWhiteSpace(strategy.Symbol))
                throw new InvalidOperationException("Strategy symbol is required.");

            if (strategy.KlineInterval == default)
                throw new InvalidOperationException("Strategy kline interval is required.");
        }

        private static void ValidateCompatibleStrategyUpdate(Strategies.Strategy current, Strategies.Strategy updated)
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
    }
}
