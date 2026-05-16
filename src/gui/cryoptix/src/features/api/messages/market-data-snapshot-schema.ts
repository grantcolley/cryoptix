import { z } from "zod";
import { KlineSchema } from "@/features/api/schema/kline-schema";
import { TradeSchema } from "@/features/api/schema/trade-schema";
import { StrategySchema } from "@/features/api/schema/strategy-schema";
import { StrategyState } from "@/features/api/schema/strategy-state";

export const MarketDataSnapshotSchema = z.object({
  strategyState: z.enum(StrategyState).default(StrategyState.Idle),
  strategy: StrategySchema,
  snapshotTimeUtc: z.coerce.date(),
  klines: z.array(KlineSchema).default([]),
  trades: z.array(TradeSchema).default([]),
});

export type MarketDataSnapshot = z.infer<typeof MarketDataSnapshotSchema>;
