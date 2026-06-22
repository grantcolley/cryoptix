using System.Collections.Concurrent;

namespace Cryoptix.Observer.Subscription
{
    /// <summary>
    /// Represents the in memory subscription manager.
    /// </summary>
    public sealed class InMemorySubscriptionManager : ISubscriptionManager
    {
        private readonly ConcurrentDictionary<string, SubscriberConnection> _subscribers = new();

        /// <summary>
        /// Executes the register async operation.
        /// </summary>
        /// <param name="subscriber">The subscriber value.</param>
        /// <param name="cancellationToken">The cancellation token value.</param>
        /// <returns>The register async result.</returns>
        public Task RegisterAsync(
            SubscriberConnection subscriber,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(subscriber);

            _subscribers[subscriber.ConnectionId] = subscriber;
            return Task.CompletedTask;
        }

        /// <summary>
        /// Executes the unregister async operation.
        /// </summary>
        /// <param name="connectionId">The connection id value.</param>
        /// <param name="cancellationToken">The cancellation token value.</param>
        /// <returns>The unregister async result.</returns>
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

        /// <summary>
        /// Executes the get all async operation.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token value.</param>
        /// <returns>The get all async result.</returns>
        public Task<IReadOnlyCollection<SubscriberConnection>> GetAllAsync(
            CancellationToken cancellationToken = default)
        {
            IReadOnlyCollection<SubscriberConnection> result = [.. _subscribers.Values];
            return Task.FromResult(result);
        }

        /// <summary>
        /// Executes the is registered async operation.
        /// </summary>
        /// <param name="connectionId">The connection id value.</param>
        /// <param name="cancellationToken">The cancellation token value.</param>
        /// <returns>The is registered async result.</returns>
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
