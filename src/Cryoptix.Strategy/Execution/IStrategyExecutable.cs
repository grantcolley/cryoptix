using Cryoptix.Strategy.Runtime;

namespace Cryoptix.Strategy.Execution
{
    public interface IStrategyExecutable
    {
        Task ExecuteAsync(StrategyRuntime strategyRuntime, CancellationToken cancellationToken);
    }
}
