using Cryoptix.Strategy.Processor;

namespace Cryoptix.Strategy.State
{
    /// <summary>
    /// Represents the strategy status.
    /// </summary>
    public sealed class StrategyStatus
    {
        /// <summary>
        /// Gets or sets the strategy state.
        /// </summary>
        public StrategyState StrategyState { get; set; }
        /// <summary>
        /// Gets or sets the strategy processor type.
        /// </summary>
        public StrategyProcessorType StrategyProcessorType { get; set; }
        /// <summary>
        /// Gets or sets the strategy.
        /// </summary>
        public Strategies.Strategy? Strategy { get; set; }
        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        public string? Message { get; set; }
    }
}
