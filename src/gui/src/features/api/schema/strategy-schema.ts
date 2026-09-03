import { z } from "zod";
import { KlineInterval } from "./kline-interval";
import { Exchange } from "./exchange";
import { StrategyProcessorType } from "./strategy-processor-type";
import { StrategyEngineType } from "./strategy-engine-type";
import { BoundedChannelFullMode } from "./bounded-channel-full-mode";
import { PeriodSchema } from "./period-schema";

export const StrategySchema = z.object({
  // Strategy fields
  strategyId: z.number().int(),
  name: z.string().nonempty(),
  description: z.string().nullable().optional(),
  symbol: z.string().nullable().optional(),
  strategyProcessorType: z
    .enum(StrategyProcessorType)
    .default(StrategyProcessorType.None),
  strategyEngineType: z
    .enum(StrategyEngineType)
    .default(StrategyEngineType.None),
  exchange: z.enum(Exchange).default(Exchange.None),

  // Parameters for strategy logic
  periods: z.record(z.string(), PeriodSchema),

  // Subscription and caching fields
  klineInterval: z.enum(KlineInterval).default(KlineInterval.Minute),
  klineSeedSize: z.number().int(),
  klineSeedLimit: z.number().int(),
  orderBookLimit: z.number().int().nullable().default(20),
  maxOrderBookAgeSeconds: z.number().int().default(3),
  maxAccountAgeSeconds: z.number().int().default(10),
  cacheMaxKlinesPerSeries: z.number().int().default(5000),
  cacheMaxTradesPerSymbol: z.number().int().default(10000),
  cacheMaxIndicatorsPerSeries: z.number().int().default(5000),
  cacheMaxSignalsPerSeries: z.number().int().default(5000),
  subscriptionChannelKlineCapacity: z.number().int().default(500),
  subscriptionChannelKlineFullMode: z
    .enum(BoundedChannelFullMode)
    .default(BoundedChannelFullMode.DropOldest),
  subscriptionChannelTradeCapacity: z.number().int().default(10000),
  subscriptionChannelTradeFullMode: z
    .enum(BoundedChannelFullMode)
    .default(BoundedChannelFullMode.DropOldest),

  // Processing fields
  marketEventDispatcherCapacity: z.number().int().default(500),
  marketEventDispatcherFullMode: z
    .enum(BoundedChannelFullMode)
    .default(BoundedChannelFullMode.DropOldest),

  // Broadcast fields
  klineBroadcastCapacity: z.number().int().default(500),
  klineBroadcastFullMode: z
    .enum(BoundedChannelFullMode)
    .default(BoundedChannelFullMode.DropOldest),
  tradeBroadcastCapacity: z.number().int().default(10000),
  tradeBroadcastFullMode: z
    .enum(BoundedChannelFullMode)
    .default(BoundedChannelFullMode.DropOldest),
  indicatorsBroadcastCapacity: z.number().int().default(5000),
  indicatorsBroadcastFullMode: z
    .enum(BoundedChannelFullMode)
    .default(BoundedChannelFullMode.DropOldest),
  signalBroadcastCapacity: z.number().int().default(5000),
  signalBroadcastFullMode: z
    .enum(BoundedChannelFullMode)
    .default(BoundedChannelFullMode.DropOldest),
  broadcastQueueCapacity: z.number().int().default(10000),
  broadcastQueueFullMode: z
    .enum(BoundedChannelFullMode)
    .default(BoundedChannelFullMode.DropOldest),
});

export type Strategy = z.infer<typeof StrategySchema>;
