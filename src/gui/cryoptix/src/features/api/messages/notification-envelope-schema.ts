import { z } from "zod";
import { KlineSchema } from "@/features/api/schema/kline-schema";
import { TradeSchema } from "@/features/api/schema/trade-schema";
import { StrategySchema } from "@/features/api/schema/strategy-schema";
import { SignalSchema } from "@/features/api/schema/signal-schema";
import { IndicatorsSchema } from "@/features/api/schema/indicators-schema";
import { MarketDataSnapshotSchema } from "./market-data-snapshot-schema";
import { MessageType } from "./message-type";

export const NotificationPayloadSchema = z.union([
  TradeSchema,
  KlineSchema,
  MarketDataSnapshotSchema,
  StrategySchema,
  IndicatorsSchema,
  SignalSchema,
]);

export const NotificationEnvelopeSchema = z.object({
  messageType: z.enum(MessageType).default(MessageType.None),
  timestampUtc: z.coerce.date(),
  payload: NotificationPayloadSchema.nullable().optional(),
});

export type NotificationEnvelope = z.infer<typeof NotificationEnvelopeSchema>;
