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
  open: z.string(),
  high: z.string(),
  low: z.string(),
  close: z.string(),
  volume: z.string(),
  quoteAssetVolume: z.string(),
  takerBuyBaseAssetVolume: z.string(),
  takerBuyQuoteAssetVolume: z.string(),

  final: z.boolean(),
  numberOfTrades: z.number().int(),
});

export type Kline = z.infer<typeof KlineSchema>;
