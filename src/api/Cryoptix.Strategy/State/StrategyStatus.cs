using Cryoptix.Strategy.Processor;

namespace Cryoptix.Strategy.State
{
    public class StrategyStatus
    {
        public StrategyState StrategyState { get; set; }
        public StrategyProcessorType StrategyProcessorType { get; set; }
        public Runtime.Strategy? Strategy { get; set; }
        public string? Message { get; set; }
    }
}
