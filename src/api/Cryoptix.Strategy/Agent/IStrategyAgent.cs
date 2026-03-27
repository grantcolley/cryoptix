namespace Cryoptix.Strategy.Agent
{
    public interface IStrategyAgent : IAsyncDisposable
    {
        Task StartAsync(Runtime.Strategy strategy, CancellationToken cancellationToken);
        Task StopAsync();
        Task UpdateAsync(Runtime.Strategy strategy);
    }
}
