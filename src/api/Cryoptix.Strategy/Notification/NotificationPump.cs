using Cryoptix.Market.Data;
using Cryoptix.Observer.Metrics;
using Cryoptix.Observer.Notification;
using Cryoptix.Strategy.Channel;
using Microsoft.Extensions.Logging;
using System.Threading.Channels;

namespace Cryoptix.Strategy.Notification
{
    /// <summary>
    /// Pumps notifications from strategy event channels to the notification dispatcher.
    /// </summary>
    /// <param name="logger">Logger used for diagnostic messages.</param>
    /// <param name="notificationMetrics">Metrics recorder for notification publish results.</param>
    /// <param name="notificationDispatcher">Dispatcher used to publish notifications.</param>
    public sealed class NotificationPump(
        ILogger<NotificationPump> logger,
        INotificationMetrics notificationMetrics,
        INotificationDispatcher notificationDispatcher) : INotificationPump
    {
        private readonly ILogger<NotificationPump> _logger = logger;
        private readonly INotificationMetrics _notificationMetrics = notificationMetrics;
        private readonly INotificationDispatcher _notificationDispatcher = notificationDispatcher;

        /// <summary>
        /// Runs the notification pump, reading broadcasted klines and trades from the provided channels
        /// and publishing them via the configured <see cref="INotificationDispatcher"/> until cancellation.
        /// </summary>
        /// <param name="strategy">The strategy for which to publish notifications.</param>
        /// <param name="channels">Strategy event channels containing kline and trade broadcast channels.</param>
        /// <param name="cancellationToken">Cancellation token to observe for graceful shutdown.</param>
        /// <returns>A task that completes when the pump stops due to cancellation or channels completion.</returns>
        public async Task RunAsync(
            Strategies.Strategy strategy,
            StrategyEventChannels channels,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(channels);

            ChannelReader<Kline> klineReader = channels.KlineBroadcasts.Reader;
            ChannelReader<Trade> tradeReader = channels.TradeBroadcasts.Reader;
            ChannelReader<Market.Strategy.Indicators> indicatorsReader = channels.IndicatorsBroadcasts.Reader;
            ChannelReader<Market.Strategy.Signal> signalReader = channels.SignalBroadcasts.Reader;

            while (!cancellationToken.IsCancellationRequested)
            {
                bool processedAny = false;

                while (klineReader.TryRead(out Kline? kline))
                {
                    processedAny = true;

                    try
                    {
                        await _notificationDispatcher.PublishAsync(kline, cancellationToken);
                    }
                    catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                    {
                        throw;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(
                            ex,
                            "Failed to publish kline notification for {Symbol} {Interval}",
                            kline.Symbol,
                            kline.Interval);

                        _notificationMetrics.RecordPublishFailureKline(kline.Symbol, kline.Interval, ex);
                    }
                }

                while (indicatorsReader.TryRead(out Market.Strategy.Indicators? indicators))
                {
                    processedAny = true;

                    try
                    {
                        await _notificationDispatcher.PublishAsync(indicators, cancellationToken);
                    }
                    catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                    {
                        throw;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(
                            ex,
                            "Failed to publish indicator notification for {Symbol}",
                            indicators.TimestampUtc);

                        _notificationMetrics.RecordPublishFailureIndicator(indicators is null ? null : null, ex);
                    }
                }

                int maxTradesPerPass = strategy.StrategyProcessorMaxTradesPerPass;

                int tradeBatchCount = 0;

                while (tradeBatchCount < maxTradesPerPass && tradeReader.TryRead(out Trade? trade))
                {
                    processedAny = true;
                    tradeBatchCount++;

                    try
                    {
                        await _notificationDispatcher.PublishAsync(trade, cancellationToken);
                    }
                    catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                    {
                        throw;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(
                            ex,
                            "Failed to publish trade notification for {Symbol} TradeId:{TradeId}",
                            trade.Symbol,
                            trade.Id);

                        _notificationMetrics.RecordPublishFailureTrade(trade.Symbol, ex);
                    }
                }

                while (signalReader.TryRead(out Market.Strategy.Signal? signal))
                {
                    processedAny = true;

                    try
                    {
                        await _notificationDispatcher.PublishAsync(signal, cancellationToken);
                    }
                    catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                    {
                        throw;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(
                            ex,
                            "Failed to publish signal notification for {Symbol}",
                            signal.TimestampUtc);

                        _notificationMetrics.RecordPublishFailureSignal(signal is null ? null : null, ex);
                    }
                }

                if (klineReader.Completion.IsCompleted && tradeReader.Completion.IsCompleted && indicatorsReader.Completion.IsCompleted && signalReader.Completion.IsCompleted)
                {
                    bool hasRemainingKlines = klineReader.TryPeek(out _);
                    bool hasRemainingTrades = tradeReader.TryPeek(out _);
                    bool hasRemainingIndicators = indicatorsReader.TryPeek(out _);
                    bool hasRemainingSignals = signalReader.TryPeek(out _);

                    if (!hasRemainingKlines && !hasRemainingTrades && !hasRemainingIndicators && !hasRemainingSignals)
                        break;
                }

                if (processedAny)
                    continue;

                Task<bool> waitForKline = klineReader.WaitToReadAsync(cancellationToken).AsTask();
                Task<bool> waitForTrade = tradeReader.WaitToReadAsync(cancellationToken).AsTask();
                Task<bool> waitForIndicator = indicatorsReader.WaitToReadAsync(cancellationToken).AsTask();
                Task<bool> waitForSignal = signalReader.WaitToReadAsync(cancellationToken).AsTask();

                Task completed = await Task.WhenAny(waitForKline, waitForTrade, waitForIndicator, waitForSignal);

                if (completed == waitForKline)
                {
                    await waitForKline;
                }
                else if (completed == waitForTrade)
                {
                    await waitForTrade;
                }
                else if (completed == waitForIndicator)
                {
                    await waitForIndicator;
                }
                else
                {
                    await waitForSignal;
                }
            }
        }
    }
}
