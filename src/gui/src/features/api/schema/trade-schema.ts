import { z } from "zod";
import { Exchange } from "./exchange";

export const TradeSchema = z.object({
  symbol: z.string().nullable().optional(),
  exchange: z.enum(Exchange).default(Exchange.None),
  time: z.coerce.date(),
  id: z.number().int(),
  price: z.number(),
  baseQuantity: z.number(),
  quoteQuantity: z.number(),
});

export type Trade = z.infer<typeof TradeSchema>;
