import { z } from "zod";
import { Exchange } from "./exchange";
import { KlineInterval } from "./kline-interval";

export const KlineSchema = z.object({
  symbol: z.string().nullable().optional(),
  exchange: z.enum(Exchange).default(Exchange.None),
  interval: z.enum(KlineInterval).default(KlineInterval.Unknown),
  openTime: z.coerce.date(),
  closeTime: z.coerce.date(),

  // decimal -> string for exact precision
  open: z.number(),
  high: z.number(),
  low: z.number(),
  close: z.number(),
  volume: z.number(),
  quoteAssetVolume: z.number(),
  takerBuyBaseAssetVolume: z.number(),
  takerBuyQuoteAssetVolume: z.number(),

  final: z.boolean(),
  numberOfTrades: z.number().int(),
});

export type Kline = z.infer<typeof KlineSchema>;
