using Cryoptix.Strategy.Channel;

namespace Cryoptix.Strategy.Notification
{
    /// <summary>
    /// Defines the notification pump contract.
    /// </summary>
    public interface INotificationPump
    {
        /// <summary>
        /// Starts the notification pump which reads events from the provided channels and publishes notifications.
        /// </summary>
        /// <param name="strategy">The strategy for which to publish notifications.</param>
        /// <param name="channels">The strategy event channels to read from.</param>
        /// <param name="cancellationToken">A token to observe while waiting for the pump to finish.</param>
        /// <returns>A task that completes when the pump stops.</returns>
        Task RunAsync(Strategies.Strategy strategy, StrategyEventChannels channels, CancellationToken cancellationToken);
    }
}
