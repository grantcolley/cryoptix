using Cryoptix.Market.Data;

namespace Cryoptix.Observer.Metrics
{
    /// <summary>
    /// Defines the i notification metrics contract.
    /// </summary>
    public interface INotificationMetrics
    {
        void RecordBroadcastDropKline(string? symbol, KlineInterval interval);
        void RecordBroadcastDropTrade(string? symbol);

        void RecordPublishFailureKline(string? symbol, KlineInterval interval, Exception exception);
        void RecordPublishFailureTrade(string? symbol, Exception exception);
        void RecordPublishFailureIndicator(string? symbol, Exception exception);
        void RecordPublishFailureSignal(string? symbol, Exception exception);

        void RecordNotificationLagKline(string? symbol, KlineInterval interval, TimeSpan lag);
        void RecordNotificationLagTrade(string? symbol, TimeSpan lag);

        void RecordPublishDurationKline(string? symbol, KlineInterval interval, TimeSpan duration);
        void RecordPublishDurationTrade(string? symbol, TimeSpan duration);
        void RecordPublishDurationIndicator(string? symbol, TimeSpan duration);
        void RecordPublishDurationSignal(string? symbol, TimeSpan duration);
    }
}
