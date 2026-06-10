import { z } from "zod";

export const IndicatorValueSchema = z.object({
  key: z.string(),
  value: z.number(),
});

const IndicatorValuesSchema = z
  .union([z.array(IndicatorValueSchema), z.record(z.string(), z.number())])
  .transform((values) =>
    Array.isArray(values)
      ? values
      : Object.entries(values).map(([key, value]) => ({ key, value }))
  );

export const IndicatorsSchema = z.object({
  timestampUtc: z.coerce.date(),
  values: IndicatorValuesSchema,
});

export type IndicatorValue = z.infer<typeof IndicatorValueSchema>;
export type Indicators = z.infer<typeof IndicatorsSchema>;
