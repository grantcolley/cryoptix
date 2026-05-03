namespace Cryoptix.Strategy.Agent
{
    public interface IStrategyAgent : IAsyncDisposable
    {
        Task StartAsync(Strategies.Strategy strategy, CancellationToken cancellationToken);
        Task StopAsync();
        Task UpdateAsync(Strategies.Strategy strategy);
    }
}
