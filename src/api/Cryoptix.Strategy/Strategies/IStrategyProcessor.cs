using Cryoptix.Strategy.Runtime;

namespace Cryoptix.Strategy.Strategies
{
    public interface IStrategyProcessor
    {
        Task ExecuteAsync(StrategyRuntime strategyRuntime, CancellationToken cancellationToken);
    }
}
