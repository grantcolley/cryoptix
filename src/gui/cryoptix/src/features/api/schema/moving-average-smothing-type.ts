import { z } from "zod";

export const MovingAverageSmoothingType = {
  Sma: 0,
  Ema: 1,
} as const;

export const MovingAverageSmoothingTypeSchema = z.enum(
  MovingAverageSmoothingType
);

export type MovingAverageSmoothingType = z.infer<
  typeof MovingAverageSmoothingTypeSchema
>;

export const MovingAverageSmoothingTypeLabels: Record<
  MovingAverageSmoothingType,
  string
> = {
  [MovingAverageSmoothingType.Sma]: "SMA",
  [MovingAverageSmoothingType.Ema]: "EMA",
};
