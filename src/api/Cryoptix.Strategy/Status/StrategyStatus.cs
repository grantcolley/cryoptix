using Cryoptix.Strategy.Strategies;

namespace Cryoptix.Strategy.Status
{
    public class StrategyStatus
    {
        public StrategyState StrategyState { get; set; }
        public StrategyProcessorType StrategyProcessorType { get; set; }
        public Runtime.Strategy? Strategy { get; set; }
        public string? Message { get; set; }
    }
}
