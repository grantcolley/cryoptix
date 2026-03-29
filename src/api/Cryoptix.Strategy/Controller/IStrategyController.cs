using Cryoptix.Strategy.Command;
using Cryoptix.Strategy.Processor;
using Cryoptix.Strategy.Status;

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
