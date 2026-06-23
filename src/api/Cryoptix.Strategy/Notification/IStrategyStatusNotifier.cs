namespace Cryoptix.Strategy.Notification
{
    /// <summary>
    /// Defines the i strategy status notifier contract.
    /// </summary>
    public interface IStrategyStatusNotifier
    {
        Task NotifyStartedAsync(Strategies.Strategy strategy, CancellationToken cancellationToken);
        Task NotifyMarketDataSnapshotAsync(CancellationToken cancellationToken);
        Task NotifyUpdatedAsync(Strategies.Strategy strategy, CancellationToken cancellationToken);
    }
}
