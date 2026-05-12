import { z } from "zod";
import { KlineSchema } from "@/features/api/schema/kline-schema";
import { TradeSchema } from "@/features/api/schema/trade-schema";
import { KlineInterval } from "@/features/api/schema/kline-interval";

export const MarketDataSnapshotSchema = z.object({
  symbol: z.string(),
  interval: z.enum(KlineInterval).default(KlineInterval.Unknown),
  snapshotTimeUtc: z.coerce.date(),
  klines: z.array(KlineSchema).default([]),
  trades: z.array(TradeSchema).default([]),
});

export type MarketDataSnapshot = z.infer<typeof MarketDataSnapshotSchema>;
