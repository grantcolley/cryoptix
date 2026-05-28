import { z } from "zod";

export const MovingAverageSmoothingType = {
  None: 0,
  Sma: 1,
  Ema: 2,
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
  [MovingAverageSmoothingType.None]: "None",
  [MovingAverageSmoothingType.Sma]: "SMA",
  [MovingAverageSmoothingType.Ema]: "EMA",
};
