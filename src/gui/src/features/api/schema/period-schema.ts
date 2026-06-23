import { z } from "zod";
import { MovingAverageSmoothingType } from "./moving-average-smothing-type";

export const PeriodSchema = z.object({
  name: z.string().nullable().optional(),
  value: z.number().int(),
  smoothingType: z
    .enum(MovingAverageSmoothingType)
    .default(MovingAverageSmoothingType.Sma),
});

export type Period = z.infer<typeof PeriodSchema>;
