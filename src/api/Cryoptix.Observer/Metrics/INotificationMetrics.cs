using Cryoptix.Market.Data;

namespace Cryoptix.Observer.Metrics
{
    /// <summary>
    /// Defines the notification metrics contract.
    /// </summary>
    public interface INotificationMetrics
    {
        /// <summary>
        /// Records the broadcast drop kline.
        /// </summary>
        /// <param name="symbol">The symbol.</param>
        /// <param name="interval">The interval.</param>
        void RecordBroadcastDropKline(string? symbol, KlineInterval interval);
        /// <summary>
        /// Records the broadcast drop trade.
        /// </summary>
        /// <param name="symbol">The symbol.</param>
        void RecordBroadcastDropTrade(string? symbol);

        /// <summary>
        /// Records the publish failure kline.
        /// </summary>
        /// <param name="symbol">The symbol.</param>
        /// <param name="interval">The interval.</param>
        /// <param name="exception">The exception.</param>
        void RecordPublishFailureKline(string? symbol, KlineInterval interval, Exception exception);
        /// <summary>
        /// Records the publish failure trade.
        /// </summary>
        /// <param name="symbol">The symbol.</param>
        /// <param name="exception">The exception.</param>
        void RecordPublishFailureTrade(string? symbol, Exception exception);
        /// <summary>
        /// Records the publish failure indicator.
        /// </summary>
        /// <param name="symbol">The symbol.</param>
        /// <param name="exception">The exception.</param>
        void RecordPublishFailureIndicator(string? symbol, Exception exception);
        /// <summary>
        /// Records the publish failure signal.
        /// </summary>
        /// <param name="symbol">The symbol.</param>
        /// <param name="exception">The exception.</param>
        void RecordPublishFailureSignal(string? symbol, Exception exception);

        /// <summary>
        /// Records the notification lag kline.
        /// </summary>
        /// <param name="symbol">The symbol.</param>
        /// <param name="interval">The interval.</param>
        /// <param name="lag">The lag.</param>
        void RecordNotificationLagKline(string? symbol, KlineInterval interval, TimeSpan lag);
        /// <summary>
        /// Records the notification lag trade.
        /// </summary>
        /// <param name="symbol">The symbol.</param>
        /// <param name="lag">The lag.</param>
        void RecordNotificationLagTrade(string? symbol, TimeSpan lag);

        /// <summary>
        /// Records the publish duration kline.
        /// </summary>
        /// <param name="symbol">The symbol.</param>
        /// <param name="interval">The interval.</param>
        /// <param name="duration">The duration.</param>
        void RecordPublishDurationKline(string? symbol, KlineInterval interval, TimeSpan duration);
        /// <summary>
        /// Records the publish duration trade.
        /// </summary>
        /// <param name="symbol">The symbol.</param>
        /// <param name="duration">The duration.</param>
        void RecordPublishDurationTrade(string? symbol, TimeSpan duration);
        /// <summary>
        /// Records the publish duration indicator.
        /// </summary>
        /// <param name="symbol">The symbol.</param>
        /// <param name="duration">The duration.</param>
        void RecordPublishDurationIndicator(string? symbol, TimeSpan duration);
        /// <summary>
        /// Records the publish duration signal.
        /// </summary>
        /// <param name="symbol">The symbol.</param>
        /// <param name="duration">The duration.</param>
        void RecordPublishDurationSignal(string? symbol, TimeSpan duration);
    }
}
