import { z } from "zod";

export const BoundedChannelFullMode = {
  Wait: 0,
  DropNewest: 1,
  DropOldest: 2,
  DropWrite: 3,
} as const;

export const BoundedChannelFullModeSchema = z.enum(BoundedChannelFullMode);

export type BoundedChannelFullMode = z.infer<
  typeof BoundedChannelFullModeSchema
>;
