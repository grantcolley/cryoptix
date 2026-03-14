namespace Cryoptix.Strategy.Execution
{
    public interface IStrategyExecution : IAsyncDisposable
    {
        Task StartAsync(Runtime.Strategy strategy, CancellationToken cancellationToken);
        Task StopAsync();
        Task UpdateAsync(Runtime.Strategy strategy);
    }
}
