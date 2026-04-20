using Cryoptix.Exchange.Models;
using Cryoptix.Observer.Metrics;
using Cryoptix.Observer.Notification;
using Cryoptix.Strategy.Channel;
using Microsoft.Extensions.Logging;
using System.Threading.Channels;

namespace Cryoptix.Strategy.Notification
{
    public sealed class NotificationPump(
        ILogger<NotificationPump> logger,
        INotificationMetrics notificationMetrics,
        INotificationDispatcher notificationDispatcher) : INotificationPump
    {
        private readonly ILogger<NotificationPump> _logger = logger;
        private readonly INotificationMetrics _notificationMetrics = notificationMetrics;
        private readonly INotificationDispatcher _notificationDispatcher = notificationDispatcher;

        public async Task RunAsync(
            StrategyEventChannels channels,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(channels);

            ChannelReader<Kline> klineReader = channels.KlineBroadcasts.Reader;
            ChannelReader<Trade> tradeReader = channels.TradeBroadcasts.Reader;

            const int maxTradesPerPass = 256;

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

                if (klineReader.Completion.IsCompleted && tradeReader.Completion.IsCompleted)
                {
                    bool hasRemainingKlines = klineReader.TryPeek(out _);
                    bool hasRemainingTrades = tradeReader.TryPeek(out _);

                    if (!hasRemainingKlines && !hasRemainingTrades)
                        break;
                }

                if (processedAny)
                    continue;

                Task<bool> waitForKline = klineReader.WaitToReadAsync(cancellationToken).AsTask();
                Task<bool> waitForTrade = tradeReader.WaitToReadAsync(cancellationToken).AsTask();

                Task completed = await Task.WhenAny(waitForKline, waitForTrade);

                if (completed == waitForKline)
                {
                    await waitForKline;
                }
                else
                {
                    await waitForTrade;
                }
            }
        }
    }
}
