using Cryoptix.Strategy.Command;
using Cryoptix.Strategy.Processor;
using Cryoptix.Strategy.State;

namespace Cryoptix.Strategy.Controller
{
    public interface IStrategyController
    {
        StrategyStatus GetStatus();
        IReadOnlyCollection<StrategyProcessorType> GetAvailableStrategies();
        Task<StrategyCommandResult> StartAsync(Strategies.Strategy strategy, CancellationToken ct);
        Task<StrategyCommandResult> UpdateAsync(Strategies.Strategy strategy, CancellationToken ct);
        Task<StrategyCommandResult> StopAsync(CancellationToken ct);
    }
}
