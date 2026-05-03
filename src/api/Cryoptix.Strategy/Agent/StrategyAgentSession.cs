using Cryoptix.Exchange.Api;
using Cryoptix.Market.Data;

namespace Cryoptix.Strategy.Agent
{
    public class StrategyAgentSession
    {
        public Func<Strategies.Strategy>? GetStrategy { get; init; }
        public required Func<CancellationToken, Task> WaitForStrategyUpdateAsync { get; init; }
        public ExchangeApi? ExchangeApi { get; init; }
        public Credentials? Credentials { get; init; }
    }
}
