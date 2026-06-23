using Cryoptix.Exchange.Api;
using Cryoptix.Market.Data;

namespace Cryoptix.Strategy.Agent
{
    /// <summary>
    /// Represents the strategy agent session.
    /// </summary>
    public sealed class StrategyAgentSession
    {
        /// <summary>
        /// Gets or sets the get strategy.
        /// </summary>
        public Func<Strategies.Strategy>? GetStrategy { get; init; }
        /// <summary>
        /// Gets or sets the wait for strategy update async.
        /// </summary>
        public required Func<CancellationToken, Task> WaitForStrategyUpdateAsync { get; init; }
        /// <summary>
        /// Gets or sets the exchange api.
        /// </summary>
        public ExchangeApi? ExchangeApi { get; init; }
        /// <summary>
        /// Gets or sets the credentials.
        /// </summary>
        public Credentials? Credentials { get; init; }
    }
}
