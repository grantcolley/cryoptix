import { z } from "zod";

export const StrategyEngineType = {
  None: 0,
  MovingAverage: 1,
} as const;

export const StrategyEngineTypeSchema = z.enum(StrategyEngineType);

export type StrategyEngineType = z.infer<typeof StrategyEngineTypeSchema>;

export const StrategyEngineTypeLabels: Record<StrategyEngineType, string> = {
  [StrategyEngineType.None]: "None",
  [StrategyEngineType.MovingAverage]: "Moving Average",
};
