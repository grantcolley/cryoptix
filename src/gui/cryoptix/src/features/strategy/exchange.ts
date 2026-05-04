import { z } from "zod";

export const Exchange = {
  None: 0,
  Binance: 1,
} as const;

export const ExchangeSchema = z.enum(Exchange);

export type Exchange = z.infer<typeof ExchangeSchema>;
