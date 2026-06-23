namespace Cryoptix.Strategy.Agent
{
    /// <summary>
    /// Defines the i strategy agent contract.
    /// </summary>
    public interface IStrategyAgent : IAsyncDisposable
    {
        Task StartAsync(Strategies.Strategy strategy, CancellationToken cancellationToken);
        Task StopAsync();
        Task UpdateAsync(Strategies.Strategy strategy);
    }
}
