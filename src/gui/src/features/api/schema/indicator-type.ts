import { z } from "zod";

export const IndicatorType = {
  None: 0,
  Sma: 1,
  Ema: 2,
} as const;

export const IndicatorTypeSchema = z.enum(IndicatorType);

export type IndicatorType = z.infer<typeof IndicatorTypeSchema>;

export const IndicatorTypeLabels: Record<IndicatorType, string> = {
  [IndicatorType.None]: "None",
  [IndicatorType.Sma]: "SMA",
  [IndicatorType.Ema]: "EMA",
};
