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

const NotificationEnvelopeBaseSchema = z.object({
  timestampUtc: z.coerce.date(),
});

export const NotificationEnvelopeSchema = z.discriminatedUnion("messageType", [
  NotificationEnvelopeBaseSchema.extend({
    messageType: z.literal(MessageType.None),
    payload: z.null().optional(),
  }),
  NotificationEnvelopeBaseSchema.extend({
    messageType: z.literal(MessageType.StrategyStarted),
    payload: StrategySchema,
  }),
  NotificationEnvelopeBaseSchema.extend({
    messageType: z.literal(MessageType.StrategyUpdated),
    payload: StrategySchema,
  }),
  NotificationEnvelopeBaseSchema.extend({
    messageType: z.literal(MessageType.MarketDataSnapshot),
    payload: MarketDataSnapshotSchema,
  }),
  NotificationEnvelopeBaseSchema.extend({
    messageType: z.literal(MessageType.Kline),
    payload: KlineSchema,
  }),
  NotificationEnvelopeBaseSchema.extend({
    messageType: z.literal(MessageType.Trade),
    payload: TradeSchema,
  }),
  NotificationEnvelopeBaseSchema.extend({
    messageType: z.literal(MessageType.Indicator),
    payload: IndicatorsSchema,
  }),
  NotificationEnvelopeBaseSchema.extend({
    messageType: z.literal(MessageType.Signal),
    payload: SignalSchema,
  }),
]);

export type NotificationEnvelope = z.infer<typeof NotificationEnvelopeSchema>;
