import { z } from "zod";
import { KlineSchema } from "@/features/api/schema/kline-schema";
import { TradeSchema } from "@/features/api/schema/trade-schema";
import { StrategySchema } from "@/features/api/schema/strategy-schema";

export const MarketDataSnapshotSchema = z.object({
  strategy: StrategySchema,
  snapshotTimeUtc: z.coerce.date(),
  klines: z.array(KlineSchema).default([]),
  trades: z.array(TradeSchema).default([]),
});

export type MarketDataSnapshot = z.infer<typeof MarketDataSnapshotSchema>;
