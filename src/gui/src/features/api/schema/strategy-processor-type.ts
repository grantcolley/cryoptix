import { z } from "zod";

export const StrategyProcessorType = {
  None: 0,
  TradingFlow: 1,
} as const;

export const StrategyProcessorTypeSchema = z.enum(StrategyProcessorType);

export type StrategyProcessorType = z.infer<typeof StrategyProcessorTypeSchema>;

export const StrategyProcessorTypeLabels: Record<
  StrategyProcessorType,
  string
> = {
  [StrategyProcessorType.None]: "None",
  [StrategyProcessorType.TradingFlow]: "Trading Flow",
};
