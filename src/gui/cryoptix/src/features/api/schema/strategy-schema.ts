import { z } from "zod";
import { KlineInterval } from "./kline-interval";
import { Exchange } from "./exchange";
import { StrategyProcessorType } from "./strategy-processor-type";
import { StrategyEngineType } from "./strategy-engine-type";
import { BoundedChannelFullMode } from "./bounded-channel-full-mode";

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

  // Subscription and caching fields
  klineInterval: z.enum(KlineInterval).default(KlineInterval.Minute),
  klineSeedSize: z.number().int(),
  klineSeedLimit: z.number().int(),
  fastPeriod: z.number().int(),
  slowPeriod: z.number().int(),
  orderBookLimit: z.number().int().nullable().default(20),
  maxOrderBookAgeSeconds: z.number().int().default(3),
  maxAccountAgeSeconds: z.number().int().default(10),
  cacheMaxKlinesPerSeries: z.number().int().default(5000),
  cacheMaxTradesPerSymbol: z.number().int().default(10000),
  strategyProcessorMaxTradesPerPass: z.number().int().default(256),
  subscriptionChannelKlineCapacity: z.number().int().default(500),
  subscriptionChannelTradeCapacity: z.number().int().default(10000),
  subscriptionChannelDropTradesWhenFull: z.boolean().default(true),
  subscriptionChannelKlineFullMode: z
    .enum(BoundedChannelFullMode)
    .default(BoundedChannelFullMode.Wait),

  // Broadcast fields
  klineBroadcastCapacity: z.number().int().default(500),
  tradeBroadcastCapacity: z.number().int().default(10000),
  klineBroadcastFullMode: z
    .enum(BoundedChannelFullMode)
    .default(BoundedChannelFullMode.DropOldest),
  tradeBroadcastFullMode: z
    .enum(BoundedChannelFullMode)
    .default(BoundedChannelFullMode.DropOldest),
});

export type Strategy = z.infer<typeof StrategySchema>;
