import { z } from "zod";
import { StrategySchema } from "./strategy-schema";
import { StrategyStateSchema } from "./strategy-state";
import { StrategyProcessorTypeSchema } from "./strategy-processor-type";

export const StrategyStatusApiSchema = z.object({
  StrategyState: StrategyStateSchema,
  StrategyProcessorType: StrategyProcessorTypeSchema,
  Strategy: StrategySchema.nullable(),
  Message: z.string().nullable(),
});

export const StrategyStatusSchema = StrategyStatusApiSchema.transform(
  (data) => ({
    strategyState: data.StrategyState,
    strategyProcessorType: data.StrategyProcessorType,
    strategy: data.Strategy,
    message: data.Message,
  })
);

export type StrategyStatus = z.infer<typeof StrategyStatusSchema>;
