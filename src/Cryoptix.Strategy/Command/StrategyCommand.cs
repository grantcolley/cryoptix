namespace Cryoptix.Strategy.Command
{
    public class StrategyCommand
    {
        public StrategyCommandType StrategyCommandType { get; set; }
        public Runtime.Strategy? Strategy { get; set; }
    }
}
