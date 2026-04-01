using Cryoptix.Strategy.Agent;

namespace Cryoptix.Strategy.Processor
{
    public interface IStrategyProcessor
    {
        Task ExecuteAsync(StrategyAgentSession strategyAgentSession, CancellationToken cancellationToken);
    }
}
