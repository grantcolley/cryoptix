using Cryoptix.Strategy.Agent;

namespace Cryoptix.Strategy.Processor
{
    /// <summary>
    /// Defines the i strategy processor contract.
    /// </summary>
    public interface IStrategyProcessor
    {
        Task ExecuteAsync(StrategyAgentSession strategyAgentSession, CancellationToken cancellationToken);
    }
}
