using Cryoptix.Exchange.Api;

namespace Cryoptix.Strategy.Agent
{
    public class StrategyAgentSession
    {
        public Func<Runtime.Strategy>? GetStrategy { get; init; }
        public required Func<CancellationToken, Task> WaitForStrategyUpdateAsync { get; init; }
        public ExchangeApi? ExchangeApi { get; init; }
    }
}
