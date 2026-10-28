import { z } from "zod";

export const MessageType = {
  None: 0,
  StrategyStarted: 1,
  StrategyUpdated: 2,
  MarketDataSnapshot: 3,
  Kline: 4,
  Trade: 5,
  OrderBook: 6,
  Indicators: 7,
  Signal: 8,
} as const;

export const MessageTypeSchema = z.enum(MessageType);

export type MessageType = z.infer<typeof MessageTypeSchema>;
