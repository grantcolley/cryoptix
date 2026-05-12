import { z } from "zod";

export const MessageType = {
  None: 0,
  MarketDataSnapshot: 1,
  Kline: 2,
  Trade: 3,
  StrategyUpdated: 4,
} as const;

export const MessageTypeSchema = z.enum(MessageType);

export type MessageType = z.infer<typeof MessageTypeSchema>;
