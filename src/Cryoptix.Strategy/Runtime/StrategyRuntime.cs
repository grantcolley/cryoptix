using Cryoptix.Core.Api;

namespace Cryoptix.Strategy.Runtime
{
    public class StrategyRuntime
    {
        public Func<Strategy>? GetStrategy { get; init; }
        public required Func<CancellationToken, Task> WaitForStrategyUpdateAsync { get; init; }
        public ExchangeApi? ExchangeApi { get; init; }
    }
}
