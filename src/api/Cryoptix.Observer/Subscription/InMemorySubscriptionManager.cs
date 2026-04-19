using System.Collections.Concurrent;

namespace Cryoptix.Observer.Subscription
{
    public sealed class InMemorySubscriptionManager : ISubscriptionManager
    {
        private readonly ConcurrentDictionary<string, SubscriberConnection> _subscribers = new();

        public Task RegisterAsync(
            SubscriberConnection subscriber,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(subscriber);

            _subscribers[subscriber.ConnectionId] = subscriber;
            return Task.CompletedTask;
        }

        public Task UnregisterAsync(
            string connectionId,
            CancellationToken cancellationToken = default)
        {
            if (!string.IsNullOrWhiteSpace(connectionId))
            {
                _subscribers.TryRemove(connectionId, out _);
            }

            return Task.CompletedTask;
        }

        public Task<IReadOnlyCollection<SubscriberConnection>> GetAllAsync(
            CancellationToken cancellationToken = default)
        {
            IReadOnlyCollection<SubscriberConnection> result = _subscribers.Values.ToArray();
            return Task.FromResult(result);
        }

        public Task<bool> IsRegisteredAsync(
            string connectionId,
            CancellationToken cancellationToken = default)
        {
            var exists = !string.IsNullOrWhiteSpace(connectionId) &&
                         _subscribers.ContainsKey(connectionId);

            return Task.FromResult(exists);
        }
    }
}
