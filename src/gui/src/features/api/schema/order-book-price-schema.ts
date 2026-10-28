import { z } from "zod";

export const OrderBookPriceSchema = z.object({
  price: z.number(),
  quantity: z.number(),
});

export type OrderBookPrice = z.infer<typeof OrderBookPriceSchema>;
