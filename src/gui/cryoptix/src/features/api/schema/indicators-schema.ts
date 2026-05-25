import { z } from "zod";

export const IndicatorValueSchema = z.object({
  key: z.string(),
  value: z.number(),
});

export const IndicatorsSchema = z.object({
  timestampUtc: z.coerce.date(),
  values: z.array(IndicatorValueSchema),
});

export type IndicatorValue = z.infer<typeof IndicatorValueSchema>;
export type Indicators = z.infer<typeof IndicatorsSchema>;
