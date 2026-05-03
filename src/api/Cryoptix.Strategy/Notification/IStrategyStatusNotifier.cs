namespace Cryoptix.Strategy.Notification
{
    public interface IStrategyStatusNotifier
    {
        Task NotifyUpdatedAsync(Strategies.Strategy strategy, CancellationToken cancellationToken);
    }
}
