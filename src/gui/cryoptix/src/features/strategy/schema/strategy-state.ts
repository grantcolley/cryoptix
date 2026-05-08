import { z } from "zod";

export const StrategyState = {
  Idle: 0,
  Starting: 1,
  Running: 2,
  Stopping: 3,
  Faulted: 4,
} as const;

export const StrategyStateSchema = z.enum(StrategyState);

export type StrategyState = z.infer<typeof StrategyStateSchema>;
