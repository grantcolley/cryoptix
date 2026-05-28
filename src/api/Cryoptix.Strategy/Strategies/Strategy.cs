using Cryoptix.Market.Data;
using Cryoptix.Strategy.Engine.MovingAverage;
using Cryoptix.Strategy.Processor;
using System.Threading.Channels;

namespace Cryoptix.Strategy.Strategies
{
    public class Strategy
    {
        // Basic strategy info
        public int StrategyId { get; set; } = 1;
        public string? Name { get; set; } = "Moving Average Crossover";
        public string? Description { get; set; } = "A simple moving average crossover strategy.";
        public string? Symbol { get; set; } = "BTCUSDT";
        public StrategyProcessorType StrategyProcessorType { get; set; } = StrategyProcessorType.TradingFlow;
        public StrategyEngineType StrategyEngineType { get; set; } = StrategyEngineType.MovingAverage;
        public Market.Data.Exchange Exchange { get; set; } = Market.Data.Exchange.Binance;

        // Subscription and cache settings
        public KlineInterval KlineInterval { get; set; } = KlineInterval.Minute;
        public int KlineSeedSize { get; set; } = 480; // e.g. 1-minute klines: 480 = 8 hours, 720 = 12 hours, 1440 = 1 day
        public int KlineSeedLimit { get; set; } = 1000; // Max klines to fetch per API call during seeding
        public int? OrderBookLimit { get; set; } = 20;
        public int MaxOrderBookAgeSeconds { get; set; } = 3;
        public int MaxAccountAgeSeconds { get; set; } = 10;
        public int CacheMaxKlinesPerSeries { get; set; } = 5000;
        public int CacheMaxTradesPerSymbol { get; set; } = 10000;
        public int CacheMaxIndicatorsPerSeries { get; set; } = 5000;
        public int CacheMaxSignalsPerSeries { get; set; } = 5000;
        public int StrategyProcessorMaxTradesPerPass { get; set; } = 256;
        public int SubscriptionChannelKlineCapacity { get; set; } = 10000;
        public int SubscriptionChannelTradeCapacity { get; set; } = 10000;
        public BoundedChannelFullMode SubscriptionChannelTradeFullMode { get; set; } = BoundedChannelFullMode.DropOldest;
        public BoundedChannelFullMode SubscriptionChannelKlineFullMode { get; set; } = BoundedChannelFullMode.DropOldest;

        // Broadcast settings
        public int KlineBroadcastCapacity { get; init; } = 500;
        public int TradeBroadcastCapacity { get; init; } = 10000;
        public int SignalBroadcastCapacity { get; set; } = 5000;
        public int IndicatorsBroadcastCapacity { get; set; } = 5000;
        public BoundedChannelFullMode KlineBroadcastFullMode { get; init; } = BoundedChannelFullMode.DropOldest;
        public BoundedChannelFullMode TradeBroadcastFullMode { get; init; } = BoundedChannelFullMode.DropOldest;
        public BoundedChannelFullMode SignalBroadcastFullMode { get; set; } = BoundedChannelFullMode.DropOldest;
        public BoundedChannelFullMode IndicatorsBroadcastFullMode { get; set; } = BoundedChannelFullMode.DropOldest;

        // Parameters for strategy logic
        public Dictionary<string, int> Periods { get; init; } = new Dictionary<string, int>() { { "9 SMA", 9 }, { "21 SMA", 21 }, { "50 SMA", 50 } };
        public MovingAverageSmoothingType SmoothingType { get; init; } = MovingAverageSmoothingType.Sma;
    }
}
