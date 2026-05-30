import { z } from "zod";
import { Exchange } from "./exchange";

export const SymbolSchema = z.object({
  name: z.string().nullable().optional(),
  exchange: z.enum(Exchange).default(Exchange.None),
  nameDelimiter: z.string().nullable().optional(),
  exchangeSymbol: z.string().nullable().optional(),
  baseAsset: z.string().nullable().optional(),
  baseAssetPrecision: z.number().int(),
  quoteAsset: z.string().nullable().optional(),
  quoteAssetPrecision: z.number().int(),
  notionalMinimumValue: z.number(),
  tickSize: z.number(),
  lotSize: z.number(),
});

export type Symbol = z.infer<typeof SymbolSchema>;
