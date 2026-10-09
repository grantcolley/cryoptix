import { z } from "zod";

export const IndicatorValueType = {
  Period: 0,
  SlowPeriod: 1,
  FastPeriod: 2,
  SignalPeriod: 3,
} as const;

export const IndicatorValueTypeSchema = z.enum(IndicatorValueType);

export type IndicatorValueType = z.infer<typeof IndicatorValueTypeSchema>;

export const IndicatorValueTypeLabels: Record<IndicatorValueType, string> = {
  [IndicatorValueType.Period]: "Period",
  [IndicatorValueType.SlowPeriod]: "Slow Period",
  [IndicatorValueType.FastPeriod]: "Fast Period",
  [IndicatorValueType.SignalPeriod]: "Signal Period",
};
