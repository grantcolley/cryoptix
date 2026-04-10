using Cryoptix.Exchange.Models;
using Cryoptix.Strategy.Processor;

namespace Cryoptix.Strategy.Runtime
{
    public class Strategy
    {
        public int StrategyId { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? Symbol { get; set; }
        public KlineInterval KlineInterval { get; set; }
        public StrategyProcessorType StrategyProcessorType { get; set; }
        public StrategyEngineType StrategyEngineType { get; set; }
        public Exchange.Exchanges.Exchange Exchange { get; set; }
        public int FastPeriod { get; init; }
        public int SlowPeriod { get; init; }
    }
}
