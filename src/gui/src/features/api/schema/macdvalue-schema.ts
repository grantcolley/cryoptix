import { z } from "zod";

export const MacdValueSchema = z.object({
  name: z.string().nullable().optional(),
  value: z.number(),
  signal: z.number(),
  histogram: z.number(),
});

export type MacdValue = z.infer<typeof MacdValueSchema>;
