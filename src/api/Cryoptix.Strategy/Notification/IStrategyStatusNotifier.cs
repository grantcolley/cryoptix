namespace Cryoptix.Strategy.Notification
{
    public interface IStrategyStatusNotifier
    {
        Task NotifyUpdatedAsync(Runtime.Strategy strategy, CancellationToken cancellationToken);
    }
}
