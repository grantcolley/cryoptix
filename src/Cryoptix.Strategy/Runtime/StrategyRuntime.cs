using Cryoptix.Core.Interfaces;

namespace Cryoptix.Strategy.Runtime
{
    public class StrategyRuntime
    {
        public Func<Strategy>? GetStrategy { get; init; }
        public required Func<CancellationToken, Task> WaitForStrategyUpdateAsync { get; init; }
        public IExchangeRestApi? ExchangeRestApi { get; set; }
    }
}
