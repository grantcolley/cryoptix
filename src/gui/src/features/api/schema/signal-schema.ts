import { z } from "zod";
import { SignalTypeSchema } from "./signal-type";

export const SignalSchema = z.object({
  timestampUtc: z.coerce.date(),
  signalType: SignalTypeSchema,
  reason: z.string().nullable().optional(),
});

export type Signal = z.infer<typeof SignalSchema>;
