import type { Strategy } from "@/features/api/schema/strategy-schema";

export const STRATEGY_CONFIG: Strategy[] = [
  {
    strategyId: 1,
    name: "Moving Average Crossover",
    description: "A simple moving average crossover strategy.",
    symbol: "BTCUSDT",
    strategyProcessorType: 1,
    strategyEngineType: 1,
    exchange: 1,
    klineInterval: 1,
    klineSeedSize: 1440, // 1 day of 1-minute klines
    klineSeedLimit: 1000, // Max klines to seed at once
    orderBookLimit: 20,
    maxOrderBookAgeSeconds: 3,
    maxAccountAgeSeconds: 10,
    cacheMaxKlinesPerSeries: 5000,
    cacheMaxTradesPerSymbol: 10000,
    CacheMaxIndicatorsPerSeries: 5000,
    CacheMaxSignalsPerSeries: 5000,
    strategyProcessorMaxTradesPerPass: 256,
    subscriptionChannelKlineCapacity: 1440,
    subscriptionChannelTradeCapacity: 10000,
    subscriptionChannelDropTradesWhenFull: true,
    subscriptionChannelKlineFullMode: 2,
    klineBroadcastCapacity: 5000,
    tradeBroadcastCapacity: 10000,
    klineBroadcastFullMode: 2,
    tradeBroadcastFullMode: 2,
    smoothingType: 0,
    fastPeriod: 9,
    slowPeriod: 21,
  },
];
