using Cryoptix.Market.Data;
using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Cryoptix.Observer.Metrics
{
    /// <summary>
    /// Represents the notification metrics.
    /// </summary>
    public sealed class NotificationMetrics : INotificationMetrics, IDisposable
    {
        private readonly Meter _meter;

        private readonly Counter<long> _broadcastDropCounter;
        private readonly Counter<long> _publishFailureCounter;

        private readonly Histogram<double> _notificationLagMs;
        private readonly Histogram<double> _publishDurationMs;

        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationMetrics"/> class.
        /// </summary>
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

        /// <summary>
        /// Executes the record broadcast drop kline operation.
        /// </summary>
        /// <param name="symbol">The symbol value.</param>
        /// <param name="interval">The interval value.</param>
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

        /// <summary>
        /// Executes the record broadcast drop trade operation.
        /// </summary>
        /// <param name="symbol">The symbol value.</param>
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

        /// <summary>
        /// Executes the record publish failure kline operation.
        /// </summary>
        /// <param name="symbol">The symbol value.</param>
        /// <param name="interval">The interval value.</param>
        /// <param name="exception">The exception value.</param>
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

        /// <summary>
        /// Executes the record publish failure trade operation.
        /// </summary>
        /// <param name="symbol">The symbol value.</param>
        /// <param name="exception">The exception value.</param>
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

        /// <summary>
        /// Executes the record publish failure indicator operation.
        /// </summary>
        /// <param name="symbol">The symbol value.</param>
        /// <param name="exception">The exception value.</param>
        public void RecordPublishFailureIndicator(string? symbol, Exception exception)
        {
            _publishFailureCounter.Add(
                1,
                new TagList
                {
                { "event_type", "indicator" },
                { "symbol", Normalize(symbol) },
                { "exception_type", exception.GetType().Name }
                });
        }

        /// <summary>
        /// Executes the record publish failure signal operation.
        /// </summary>
        /// <param name="symbol">The symbol value.</param>
        /// <param name="exception">The exception value.</param>
        public void RecordPublishFailureSignal(string? symbol, Exception exception)
        {
            _publishFailureCounter.Add(
                1,
                new TagList
                {
                { "event_type", "signal" },
                { "symbol", Normalize(symbol) },
                { "exception_type", exception.GetType().Name }
                });
        }

        /// <summary>
        /// Executes the record notification lag kline operation.
        /// </summary>
        /// <param name="symbol">The symbol value.</param>
        /// <param name="interval">The interval value.</param>
        /// <param name="lag">The lag value.</param>
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

        /// <summary>
        /// Executes the record notification lag trade operation.
        /// </summary>
        /// <param name="symbol">The symbol value.</param>
        /// <param name="lag">The lag value.</param>
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

        /// <summary>
        /// Executes the record publish duration kline operation.
        /// </summary>
        /// <param name="symbol">The symbol value.</param>
        /// <param name="interval">The interval value.</param>
        /// <param name="duration">The duration value.</param>
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

        /// <summary>
        /// Executes the record publish duration trade operation.
        /// </summary>
        /// <param name="symbol">The symbol value.</param>
        /// <param name="duration">The duration value.</param>
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

        /// <summary>
        /// Executes the record publish duration indicator operation.
        /// </summary>
        /// <param name="symbol">The symbol value.</param>
        /// <param name="duration">The duration value.</param>
        public void RecordPublishDurationIndicator(string? symbol, TimeSpan duration)
        {
            _publishDurationMs.Record(
                duration.TotalMilliseconds,
                new TagList
                {
                { "event_type", "indicator" },
                { "symbol", Normalize(symbol) }
                });
        }

        /// <summary>
        /// Executes the record publish duration signal operation.
        /// </summary>
        /// <param name="symbol">The symbol value.</param>
        /// <param name="duration">The duration value.</param>
        public void RecordPublishDurationSignal(string? symbol, TimeSpan duration)
        {
            _publishDurationMs.Record(
                duration.TotalMilliseconds,
                new TagList
                {
                { "event_type", "signal" },
                { "symbol", Normalize(symbol) }
                });
        }

        /// <summary>
        /// Executes the dispose operation.
        /// </summary>
        public void Dispose()
        {
            _meter.Dispose();
        }

        private static string Normalize(string? value) =>
            string.IsNullOrWhiteSpace(value) ? "unknown" : value.Trim().ToUpperInvariant();
    }
}
