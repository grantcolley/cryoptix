using Cryoptix.Market.Data;
using Cryoptix.Observer.Metrics;
using Cryoptix.Observer.Notification;
using Cryoptix.Strategy.Channel;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Threading.Channels;

namespace Cryoptix.Strategy.Notification
{
    /// <summary>
    /// Represents the notification pump.
    /// </summary>
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
        /// Executes the run async operation.
        /// </summary>
        /// <param name="strategy">The strategy value.</param>
        /// <param name="channels">The channels value.</param>
        /// <param name="cancellationToken">The cancellation token value.</param>
        /// <returns>The run async result.</returns>
        /// <summary>
        /// Runs the notification pump, reading broadcasted klines and trades from the provided channels
        /// and publishing them via the configured <see cref="INotificationDispatcher"/> until cancellation.
        /// 
        /// Fan-in pattern is used to merge multiple concurrent broadcast channels into a single serialized publish loop,
        /// ensuring that notifications are published in the order they are received from the channels.
        /// 
        /// KlineBroadcasts ─────┐
        /// TradeBroadcasts ─────┤
        /// IndicatorsBroadcasts ├──> merged broadcast channel ──> ONE PublishAsync loop
        /// SignalBroadcasts ────┘
        /// 
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
            ArgumentNullException.ThrowIfNull(strategy);
            ArgumentNullException.ThrowIfNull(channels);

            Task klineTask = ReadAsync(
                channels.KlineBroadcasts.Reader,
                channels.BroadcastQueue.Writer,
                cancellationToken);

            Task tradeTask = ReadAsync(
                channels.TradeBroadcasts.Reader,
                channels.BroadcastQueue.Writer,
                cancellationToken);

            Task indicatorsTask = ReadAsync(
                channels.IndicatorsBroadcasts.Reader,
                channels.BroadcastQueue.Writer,
                cancellationToken);

            Task signalTask = ReadAsync(
                channels.SignalBroadcasts.Reader,
                channels.BroadcastQueue.Writer,
                cancellationToken);

            Task publishTask = PublishAsync(
                channels.BroadcastQueue.Reader,
                cancellationToken);

            try
            {
                await Task.WhenAll(
                    klineTask,
                    tradeTask,
                    indicatorsTask,
                    signalTask);

                channels.BroadcastQueue.Writer.TryComplete();
            }
            catch (Exception ex)
            {
                channels.BroadcastQueue.Writer.TryComplete(ex);
                throw;
            }

            await publishTask;
        }

        private static async Task ReadAsync<T>(
            ChannelReader<T> reader,
            ChannelWriter<object> writer,
            CancellationToken cancellationToken)
        {
            await foreach (T item in reader.ReadAllAsync(cancellationToken))
            {
                await writer.WriteAsync(item!, cancellationToken);
            }
        }

        private async Task PublishAsync(
            ChannelReader<object> reader,
            CancellationToken cancellationToken)
        {
            await foreach (object notification in reader.ReadAllAsync(cancellationToken))
            {
                switch (notification)
                {
                    case Kline kline:
                        await PublishKlineAsync(kline, cancellationToken);
                        break;

                    case Trade trade:
                        await PublishTradeAsync(trade, cancellationToken);
                        break;

                    case Market.Strategy.Indicators indicators:
                        await PublishIndicatorsAsync(indicators, cancellationToken);
                        break;

                    case Market.Strategy.Signal signal:
                        await PublishSignalAsync(signal, cancellationToken);
                        break;

                    default:
                        _logger.LogWarning(
                            "Unknown notification type {NotificationType}",
                            notification.GetType().Name);
                        break;
                }
            }
        }

        private async Task PublishKlineAsync(
            Kline kline,
            CancellationToken cancellationToken)
        {
            try
            {
                await _notificationDispatcher.PublishAsync(
                    kline,
                    cancellationToken);

                _notificationMetrics.RecordNotificationLagKline(
                    kline.Symbol,
                    kline.Interval,
                    DateTime.UtcNow - kline.CloseTime);
            }
            catch (OperationCanceledException)
                when (cancellationToken.IsCancellationRequested)
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

                _notificationMetrics.RecordPublishFailureKline(
                    kline.Symbol,
                    kline.Interval,
                    ex);
            }
        }

        private async Task PublishTradeAsync(
            Trade trade,
            CancellationToken cancellationToken)
        {
            try
            {
                await _notificationDispatcher.PublishAsync(
                    trade,
                    cancellationToken);

                _notificationMetrics.RecordNotificationLagTrade(
                    trade.Symbol,
                    DateTime.UtcNow - trade.Time);
            }
            catch (OperationCanceledException)
                when (cancellationToken.IsCancellationRequested)
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

                _notificationMetrics.RecordPublishFailureTrade(
                    trade.Symbol,
                    ex);
            }
        }

        private async Task PublishIndicatorsAsync(
            Market.Strategy.Indicators indicators,
            CancellationToken cancellationToken)
        {
            try
            {
                await _notificationDispatcher.PublishAsync(
                    indicators,
                    cancellationToken);
            }
            catch (OperationCanceledException)
                when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to publish indicator notification at {TimestampUtc}",
                    indicators.TimestampUtc);

                _notificationMetrics.RecordPublishFailureIndicator(
                    null,
                    ex);
            }
        }

        private async Task PublishSignalAsync(
            Market.Strategy.Signal signal,
            CancellationToken cancellationToken)
        {
            try
            {
                await _notificationDispatcher.PublishAsync(
                    signal,
                    cancellationToken);
            }
            catch (OperationCanceledException)
                when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to publish signal notification at {TimestampUtc}",
                    signal.TimestampUtc);

                _notificationMetrics.RecordPublishFailureSignal(
                    null,
                    ex);
            }
        }
    }
}
