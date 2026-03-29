using Cryoptix.Strategy.Runtime;

namespace Cryoptix.Strategy.Processor
{
    public interface IStrategyProcessor
    {
        Task ExecuteAsync(StrategyRuntime strategyRuntime, CancellationToken cancellationToken);
    }
}
