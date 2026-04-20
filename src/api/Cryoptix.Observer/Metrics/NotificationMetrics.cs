using Cryoptix.Market.Models;
using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Cryoptix.Observer.Metrics
{
    public sealed class NotificationMetrics : INotificationMetrics, IDisposable
    {
        private readonly Meter _meter;

        private readonly Counter<long> _broadcastDropCounter;
        private readonly Counter<long> _publishFailureCounter;

        private readonly Histogram<double> _notificationLagMs;
        private readonly Histogram<double> _publishDurationMs;

        public NotificationMetrics()
        {
            _meter = new Meter("TradingFlow.Notifications", "1.0.0");

            _broadcastDropCounter = _meter.CreateCounter<long>(
                name: "notification.broadcast.dropped",
                unit: "{event}",
                description: "Number of events dropped before notification publish due to broadcast channel pressure.");

            _publishFailureCounter = _meter.CreateCounter<long>(
                name: "notification.publish.failed",
                unit: "{event}",
                description: "Number of notification publish failures.");

            _notificationLagMs = _meter.CreateHistogram<double>(
                name: "notification.lag.ms",
                unit: "ms",
                description: "Age of an event when it is published to notification subscribers.");

            _publishDurationMs = _meter.CreateHistogram<double>(
                name: "notification.publish.duration.ms",
                unit: "ms",
                description: "Time spent publishing a notification.");
        }

        public void RecordBroadcastDropKline(string? symbol, KlineInterval interval)
        {
            _broadcastDropCounter.Add(
                1,
                new TagList
                {
                { "event_type", "kline" },
                { "symbol", Normalize(symbol) },
                { "interval", interval.ToString() }
                });
        }

        public void RecordBroadcastDropTrade(string? symbol)
        {
            _broadcastDropCounter.Add(
                1,
                new TagList
                {
                { "event_type", "trade" },
                { "symbol", Normalize(symbol) }
                });
        }

        public void RecordPublishFailureKline(string? symbol, KlineInterval interval, Exception exception)
        {
            _publishFailureCounter.Add(
                1,
                new TagList
                {
                { "event_type", "kline" },
                { "symbol", Normalize(symbol) },
                { "interval", interval.ToString() },
                { "exception_type", exception.GetType().Name }
                });
        }

        public void RecordPublishFailureTrade(string? symbol, Exception exception)
        {
            _publishFailureCounter.Add(
                1,
                new TagList
                {
                { "event_type", "trade" },
                { "symbol", Normalize(symbol) },
                { "exception_type", exception.GetType().Name }
                });
        }

        public void RecordNotificationLagKline(string? symbol, KlineInterval interval, TimeSpan lag)
        {
            _notificationLagMs.Record(
                lag.TotalMilliseconds,
                new TagList
                {
                { "event_type", "kline" },
                { "symbol", Normalize(symbol) },
                { "interval", interval.ToString() }
                });
        }

        public void RecordNotificationLagTrade(string? symbol, TimeSpan lag)
        {
            _notificationLagMs.Record(
                lag.TotalMilliseconds,
                new TagList
                {
                { "event_type", "trade" },
                { "symbol", Normalize(symbol) }
                });
        }

        public void RecordPublishDurationKline(string? symbol, KlineInterval interval, TimeSpan duration)
        {
            _publishDurationMs.Record(
                duration.TotalMilliseconds,
                new TagList
                {
                { "event_type", "kline" },
                { "symbol", Normalize(symbol) },
                { "interval", interval.ToString() }
                });
        }

        public void RecordPublishDurationTrade(string? symbol, TimeSpan duration)
        {
            _publishDurationMs.Record(
                duration.TotalMilliseconds,
                new TagList
                {
                { "event_type", "trade" },
                { "symbol", Normalize(symbol) }
                });
        }

        public void Dispose()
        {
            _meter.Dispose();
        }

        private static string Normalize(string? value) =>
            string.IsNullOrWhiteSpace(value) ? "unknown" : value.Trim().ToUpperInvariant();
    }
}
