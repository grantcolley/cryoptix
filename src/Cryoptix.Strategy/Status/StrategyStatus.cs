using Cryoptix.Strategy.Strategies;

namespace Cryoptix.Strategy.Status
{
    public class StrategyStatus
    {
        public StrategyState StrategyState { get; set; }
        public StrategyType StrategyType { get; set; }
        public Runtime.Strategy? Strategy { get; set; }
        public string? Message { get; set; }
    }
}
