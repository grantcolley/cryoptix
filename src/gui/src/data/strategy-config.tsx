import type { Strategy } from "@/features/api/schema/strategy-schema";

export const STRATEGY_CONFIG: Strategy[] = [
  {
    strategyId: 1,
    name: "Moving Average Crossover",
    description:
      "A moving average crossover strategy using 9-EMA, 21-EMA, and 50-EMA.",
    symbol: "BTCUSDT",
    strategyProcessorType: 1,
    strategyEngineType: 1,
    exchange: 1,
    klineInterval: 1,
    klineSeedSize: 480,
    klineSeedLimit: 1000,
    orderBookLimit: 20,
    maxOrderBookAgeSeconds: 3,
    maxAccountAgeSeconds: 10,
    cacheMaxKlinesPerSeries: 5000,
    cacheMaxTradesPerSymbol: 10000,
    cacheMaxIndicatorsPerSeries: 5000,
    cacheMaxSignalsPerSeries: 5000,
    strategyProcessorMaxTradesPerPass: 256,
    subscriptionChannelKlineCapacity: 10000,
    subscriptionChannelTradeCapacity: 10000,
    subscriptionChannelTradeFullMode: 2,
    subscriptionChannelKlineFullMode: 2,
    klineBroadcastCapacity: 500,
    tradeBroadcastCapacity: 10000,
    signalBroadcastCapacity: 5000,
    indicatorsBroadcastCapacity: 5000,
    klineBroadcastFullMode: 2,
    tradeBroadcastFullMode: 2,
    signalBroadcastFullMode: 2,
    indicatorsBroadcastFullMode: 2,
    periods: {
      "9 EMA": {
        name: "9 EMA",
        value: 9,
        smoothingType: 2,
      },
      "21 EMA": {
        name: "21 EMA",
        value: 21,
        smoothingType: 2,
      },
      "50 EMA": {
        name: "50 EMA",
        value: 50,
        smoothingType: 2,
      },
    },
  },
];
