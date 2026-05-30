import { z } from "zod";
import { KlineSchema } from "@/features/api/schema/kline-schema";
import { TradeSchema } from "@/features/api/schema/trade-schema";
import { StrategySchema } from "@/features/api/schema/strategy-schema";
import { SignalSchema } from "@/features/api/schema/signal-schema";
import { IndicatorsSchema } from "@/features/api/schema/indicators-schema";
import { SymbolSchema } from "@/features/api/schema/symbol-schema";

export const MarketDataSnapshotSchema = z.object({
  strategy: StrategySchema,
  snapshotTimeUtc: z.coerce.date(),
  symbol: SymbolSchema,
  klines: z.array(KlineSchema).default([]),
  trades: z.array(TradeSchema).default([]),
  indicators: z.array(IndicatorsSchema).default([]),
  signals: z.array(SignalSchema).default([]),
});

export type MarketDataSnapshot = z.infer<typeof MarketDataSnapshotSchema>;
