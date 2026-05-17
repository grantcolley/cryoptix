using Cryoptix.Market.Data;
using Cryoptix.Strategy.Processor;
using System.Threading.Channels;

namespace Cryoptix.Strategy.Strategies
{
    public class Strategy
    {
        // Basic strategy info
        public int StrategyId { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? Symbol { get; set; }
        public StrategyProcessorType StrategyProcessorType { get; set; } = StrategyProcessorType.None;
        public StrategyEngineType StrategyEngineType { get; set; } = StrategyEngineType.None;
        public Market.Data.Exchange Exchange { get; set; } = Market.Data.Exchange.None;

        // Subscription and cache settings
        public KlineInterval KlineInterval { get; set; } = KlineInterval.Minute;
        public int KlineSeedSize { get; set; } = 1440; // 1 day of 1-minute klines
        public int KlineSeedLimit { get; set; } = 1000; // Max klines to fetch per API call during seeding
        public int FastPeriod { get; init; }
        public int SlowPeriod { get; init; }
        public int? OrderBookLimit { get; set; } = 20;
        public int MaxOrderBookAgeSeconds { get; set; } = 3;
        public int MaxAccountAgeSeconds { get; set; } = 10;
        public int CacheMaxKlinesPerSeries { get; set; } = 5000;
        public int CacheMaxTradesPerSymbol { get; set; } = 10000;
        public int StrategyProcessorMaxTradesPerPass { get; set; } = 256;
        public int SubscriptionChannelKlineCapacity { get; set; } = 500;
        public int SubscriptionChannelTradeCapacity { get; set; } = 10000;
        public bool SubscriptionChannelDropTradesWhenFull { get; set; } = true;
        public BoundedChannelFullMode SubscriptionChannelKlineFullMode { get; set; } = BoundedChannelFullMode.Wait;

        // Broadcast settings
        public int KlineBroadcastCapacity { get; init; } = 500;
        public int TradeBroadcastCapacity { get; init; } = 10000;
        public BoundedChannelFullMode KlineBroadcastFullMode { get; init; } = BoundedChannelFullMode.DropOldest;
        public BoundedChannelFullMode TradeBroadcastFullMode { get; init; } = BoundedChannelFullMode.DropOldest;
    }
}
