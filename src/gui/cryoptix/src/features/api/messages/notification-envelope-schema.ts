import { z } from "zod";
import { KlineSchema } from "@/features/api/schema/kline-schema";
import { TradeSchema } from "@/features/api/schema/trade-schema";
import { MarketDataSnapshotSchema } from "./market-data-snapshot-schema";
import { MessageType } from "./message-type";

// Add additional payload schemas here as needed
export const NotificationPayloadSchema = z.union([
  TradeSchema,
  KlineSchema,
  MarketDataSnapshotSchema,
]);

export const NotificationEnvelopeSchema = z.object({
  messageType: z.enum(MessageType).default(MessageType.None),
  timestampUtc: z.coerce.date(),
  payload: NotificationPayloadSchema.nullable().optional(),
});

export type NotificationEnvelope = z.infer<typeof NotificationEnvelopeSchema>;
