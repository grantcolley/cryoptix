import { z } from "zod";

export const MessageType = {
  None: 0,
  MarketDataSnapshot: 1,
  Kline: 2,
  Trade: 3,
  StrategyStarted: 4,
  StrategyUpdated: 5,
} as const;

export const MessageTypeSchema = z.enum(MessageType);

export type MessageType = z.infer<typeof MessageTypeSchema>;
