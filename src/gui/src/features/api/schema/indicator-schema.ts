import { z } from "zod";
import { IndicatorType } from "./indicator-type";

export const IndicatorSchema = z.object({
  name: z.string().nullable().optional(),
  value: z.number().int(),
  indicatorType: z.enum(IndicatorType).default(IndicatorType.Sma),
});

export type Indicator = z.infer<typeof IndicatorSchema>;
