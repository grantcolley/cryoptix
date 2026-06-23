using Cryoptix.Market.Data;
using Cryoptix.Strategy.Engine;
using Cryoptix.Strategy.Engine.MovingAverage;
using Cryoptix.Strategy.Processor;
using System.Threading.Channels;

namespace Cryoptix.Strategy.Strategies
{
    /// <summary>
    /// Represents the strategy.
    /// </summary>
    public sealed class Strategy
    {
        /// <summary>
        /// Gets or sets the strategy id.
        /// </summary>
        public int StrategyId { get; set; } = 1;
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string? Name { get; set; } = "Moving Average Crossover";
        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        public string? Description { get; set; } = "A simple moving average crossover strategy.";
        /// <summary>
        /// Gets or sets the symbol.
        /// </summary>
        public string? Symbol { get; set; } = "BTCUSDT";
        /// <summary>
        /// Gets or sets the strategy processor type.
        /// </summary>
        public StrategyProcessorType StrategyProcessorType { get; set; } = StrategyProcessorType.TradingFlow;
        /// <summary>
        /// Gets or sets the strategy engine type.
        /// </summary>
        public StrategyEngineType StrategyEngineType { get; set; } = StrategyEngineType.MovingAverage;
        /// <summary>
        /// Gets or sets the exchange.
        /// </summary>
        public Market.Data.Exchange Exchange { get; set; } = Market.Data.Exchange.Binance;
        /// Gets or sets the kline interval.
        /// </summary>
        public KlineInterval KlineInterval { get; set; } = KlineInterval.Minute;
        /// <summary>
        /// Gets or sets the kline seed size.
        /// </summary>
        public int KlineSeedSize { get; set; } = 480; // e.g. 1-minute klines: 480 = 8 hours, 720 = 12 hours, 1440 = 1 day
        /// <summary>
        /// Gets or sets the kline seed limit.
        /// </summary>
        public int KlineSeedLimit { get; set; } = 1000; // Max klines to fetch per API call during seeding
        /// <summary>
        /// Gets or sets the order book limit.
        /// </summary>
        public int? OrderBookLimit { get; set; } = 20;
        /// <summary>
        /// Gets or sets the max order book age seconds.
        /// </summary>
        public int MaxOrderBookAgeSeconds { get; set; } = 3;
        /// <summary>
        /// Gets or sets the max account age seconds.
        /// </summary>
        public int MaxAccountAgeSeconds { get; set; } = 10;
        /// <summary>
        /// Gets or sets the cache max klines per series.
        /// </summary>
        public int CacheMaxKlinesPerSeries { get; set; } = 5000;
        /// <summary>
        /// Gets or sets the cache max trades per symbol.
        /// </summary>
        public int CacheMaxTradesPerSymbol { get; set; } = 10000;
        /// <summary>
        /// Gets or sets the cache max indicators per series.
        /// </summary>
        public int CacheMaxIndicatorsPerSeries { get; set; } = 5000;
        /// <summary>
        /// Gets or sets the cache max signals per series.
        /// </summary>
        public int CacheMaxSignalsPerSeries { get; set; } = 5000;
        /// <summary>
        /// Gets or sets the strategy processor max trades per pass.
        /// </summary>
        public int StrategyProcessorMaxTradesPerPass { get; set; } = 256;
        /// <summary>
        /// Gets or sets the subscription channel kline capacity.
        /// </summary>
        public int SubscriptionChannelKlineCapacity { get; set; } = 10000;
        /// <summary>
        /// Gets or sets the subscription channel trade capacity.
        /// </summary>
        public int SubscriptionChannelTradeCapacity { get; set; } = 10000;
        /// <summary>
        /// Gets or sets the subscription channel trade full mode.
        /// </summary>
        public BoundedChannelFullMode SubscriptionChannelTradeFullMode { get; set; } = BoundedChannelFullMode.DropOldest;
        /// <summary>
        /// Gets or sets the subscription channel kline full mode.
        /// </summary>
        public BoundedChannelFullMode SubscriptionChannelKlineFullMode { get; set; } = BoundedChannelFullMode.DropOldest;
        /// <summary>
        /// Gets or sets the kline broadcast capacity.
        /// </summary>
        public int KlineBroadcastCapacity { get; init; } = 500;
        /// <summary>
        /// Gets or sets the trade broadcast capacity.
        /// </summary>
        public int TradeBroadcastCapacity { get; init; } = 10000;
        /// <summary>
        /// Gets or sets the signal broadcast capacity.
        /// </summary>
        public int SignalBroadcastCapacity { get; set; } = 5000;
        /// <summary>
        /// Gets or sets the indicators broadcast capacity.
        /// </summary>
        public int IndicatorsBroadcastCapacity { get; set; } = 5000;
        /// <summary>
        /// Gets or sets the kline broadcast full mode.
        /// </summary>
        public BoundedChannelFullMode KlineBroadcastFullMode { get; init; } = BoundedChannelFullMode.DropOldest;
        /// <summary>
        /// Gets or sets the trade broadcast full mode.
        /// </summary>
        public BoundedChannelFullMode TradeBroadcastFullMode { get; init; } = BoundedChannelFullMode.DropOldest;
        /// <summary>
        /// Gets or sets the signal broadcast full mode.
        /// </summary>
        public BoundedChannelFullMode SignalBroadcastFullMode { get; set; } = BoundedChannelFullMode.DropOldest;
        /// <summary>
        /// Gets or sets the indicators broadcast full mode.
        /// </summary>
        public BoundedChannelFullMode IndicatorsBroadcastFullMode { get; set; } = BoundedChannelFullMode.DropOldest;
        /// <summary>
        /// Gets or sets the periods.
        /// </summary>
        public Dictionary<string, int> Periods { get; init; } = new Dictionary<string, int>() { { "9 SMA", 9 }, { "21 SMA", 21 }, { "50 SMA", 50 } };
        /// <summary>
        /// Gets or sets the smoothing type.
        /// </summary>
        public MovingAverageSmoothingType SmoothingType { get; init; } = MovingAverageSmoothingType.Sma;
    }
}
