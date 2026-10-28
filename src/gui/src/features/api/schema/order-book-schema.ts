import { z } from "zod";
import { Exchange } from "./exchange";
import { OrderBookPriceSchema } from "./order-book-price-schema";

export const OrderBookSchema = z.object({
  symbol: z.string().nullable().optional(),
  exchange: z.enum(Exchange).default(Exchange.None),
  lastUpdateId: z.number().int(),
  updateTime: z.coerce.date(),
  bestAsk: OrderBookPriceSchema.nullable().optional(),
  bestBid: OrderBookPriceSchema.nullable().optional(),
  bids: z.array(OrderBookPriceSchema).nullable().optional(),
  asks: z.array(OrderBookPriceSchema).nullable().optional(),
});

export type OrderBook = z.infer<typeof OrderBookSchema>;
