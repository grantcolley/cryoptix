import { z } from "zod";
import { IndicatorValueTypeSchema } from "./indicator-value-type";

export const IndicatorValueSchema = z.object({
  type: IndicatorValueTypeSchema,
  value: z.number().int(),
});

export type IndicatorValue = z.infer<typeof IndicatorValueSchema>;
