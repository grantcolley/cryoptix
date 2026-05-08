import { z } from "zod";
import { StrategySchema } from "./strategy-schema";
import { StrategyState } from "./strategy-state";
import { StrategyProcessorType } from "./strategy-processor-type";

export const StrategyStatusApiSchema = z.object({
  strategyState: z.enum(StrategyState).default(StrategyState.Idle),

  strategyProcessorType: z
    .enum(StrategyProcessorType)
    .default(StrategyProcessorType.None),

  strategy: StrategySchema.nullable(),

  message: z.string().nullable(),
});

export const StrategyStatusSchema = StrategyStatusApiSchema;

export type StrategyStatus = z.infer<typeof StrategyStatusSchema>;
