using Cryoptix.Market.Data;

namespace Cryoptix.Observer.Notification
{
    public interface INotificationDispatcher
    {
        /// <summary>
        /// Publishes a kline notification to configured listeners.
        /// </summary>
        /// <param name="kline">The kline to publish.</param>
        /// <param name="cancellationToken">Cancellation token to observe during publish.</param>
        /// <returns>A task that completes when the publish is finished.</returns>
        Task PublishAsync(Kline kline, CancellationToken cancellationToken = default);

        /// <summary>
        /// Publishes a trade notification to configured listeners.
        /// </summary>
        /// <param name="trade">The trade to publish.</param>
        /// <param name="cancellationToken">Cancellation token to observe during publish.</param>
        /// <returns>A task that completes when the publish is finished.</returns>
        Task PublishAsync(Trade trade, CancellationToken cancellationToken = default);
    }
}
