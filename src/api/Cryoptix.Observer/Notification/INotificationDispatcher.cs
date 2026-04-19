using Cryoptix.Exchange.Models;

namespace Cryoptix.Observer.Notification
{
    public interface INotificationDispatcher
    {
        Task PublishAsync(Kline kline, CancellationToken cancellationToken = default);
        Task PublishAsync(Trade trade, CancellationToken cancellationToken = default);
    }
}
