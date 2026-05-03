namespace Cryoptix.Strategy.Command
{
    public class StrategyCommand
    {
        public StrategyCommandType StrategyCommandType { get; set; }
        public Strategies.Strategy? Strategy { get; set; }
    }
}
