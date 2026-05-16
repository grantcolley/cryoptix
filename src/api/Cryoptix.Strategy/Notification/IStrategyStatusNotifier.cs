namespace Cryoptix.Strategy.Notification
{
    public interface IStrategyStatusNotifier
    {
        Task NotifyStartedAsync(Strategies.Strategy strategy, CancellationToken cancellationToken);
        Task NotifyMarketDataSnapshotAsync(CancellationToken cancellationToken);
        Task NotifyUpdatedAsync(Strategies.Strategy strategy, CancellationToken cancellationToken);
    }
}
