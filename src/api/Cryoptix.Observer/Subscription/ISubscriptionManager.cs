namespace Cryoptix.Observer.Subscription
{
    /// <summary>
    /// Defines the i subscription manager contract.
    /// </summary>
    public interface ISubscriptionManager
    {
        Task RegisterAsync(SubscriberConnection subscriber, CancellationToken cancellationToken = default);
        Task UnregisterAsync(string connectionId, CancellationToken cancellationToken = default);
        Task<IReadOnlyCollection<SubscriberConnection>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<bool> IsRegisteredAsync(string connectionId, CancellationToken cancellationToken = default);
    }
}
