using Cryoptix.Strategy.Command;
using Cryoptix.Strategy.Status;
using Cryoptix.Strategy.Strategies;

namespace Cryoptix.Strategy.Controller
{
    public interface IStrategyController
    {
        StrategyStatus GetStatus();
        IReadOnlyCollection<StrategyProcessorType> GetAvailableStrategies();
        Task<StrategyCommandResult> StartAsync(Runtime.Strategy strategy, CancellationToken ct);
        Task<StrategyCommandResult> UpdateAsync(Runtime.Strategy strategy, CancellationToken ct);
        Task<StrategyCommandResult> StopAsync(CancellationToken ct);
    }
}
