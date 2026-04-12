using Cryoptix.Exchange.Models;
using Cryoptix.Strategy.Processor;
using System.Threading.Channels;

namespace Cryoptix.Strategy.Runtime
{
    public class Strategy
    {
        public int StrategyId { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? Symbol { get; set; }
        public KlineInterval KlineInterval { get; set; } = KlineInterval.Minute;
        public StrategyProcessorType StrategyProcessorType { get; set; } = StrategyProcessorType.TradingFlow;
        public StrategyEngineType StrategyEngineType { get; set; } = StrategyEngineType.None;
        public Exchange.Exchanges.Exchange Exchange { get; set; } = Cryoptix.Exchange.Exchanges.Exchange.Binance;
        public int FastPeriod { get; init; }
        public int SlowPeriod { get; init; }

        public int CacheMaxKlinesPerSeries { get; set; } = 5000;
        public int CacheMaxTradesPerSymbol { get; set; } = 10000;
        public int StrategyProcessorMaxTradesPerPass { get; set; } = 256;
        public int SubscriptionChannelKlineCapacity { get; set; } = 500;
        public int SubscriptionChannelTradeCapacity { get; set; } = 10000;
        public bool SubscriptionChannelDropTradesWhenFull { get; set; } = true;
        public BoundedChannelFullMode SubscriptionChannelKlineFullMode { get; set; } = BoundedChannelFullMode.Wait;
    }
}
