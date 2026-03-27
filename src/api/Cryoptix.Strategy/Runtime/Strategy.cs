using Cryoptix.Strategy.Strategies;

namespace Cryoptix.Strategy.Runtime
{
    public class Strategy
    {
        public int StrategyId { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? Symbol { get; set; }
        public StrategyProcessorType StrategyProcessorType { get; set; }
        public Exchange.Exchanges.Exchange Exchange { get; set; }
    }
}
