namespace Cryoptix.Strategy.Command
{
    public sealed class StrategyCommand
    {
        public StrategyCommandType StrategyCommandType { get; set; }
        public Strategies.Strategy? Strategy { get; set; }
    }
}
