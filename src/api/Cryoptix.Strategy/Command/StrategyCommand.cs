namespace Cryoptix.Strategy.Command
{
    /// <summary>
    /// Represents the strategy command.
    /// </summary>
    public sealed class StrategyCommand
    {
        /// <summary>
        /// Gets or sets the strategy command type.
        /// </summary>
        public StrategyCommandType StrategyCommandType { get; set; }
        /// <summary>
        /// Gets or sets the strategy.
        /// </summary>
        public Strategies.Strategy? Strategy { get; set; }
    }
}
