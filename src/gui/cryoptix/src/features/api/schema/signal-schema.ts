import { z } from "zod";
import { SignalType } from "./signal-type";

export const SignalSchema = z.object({
  timestampUtc: z.coerce.date(),
  signalType: SignalType,
  reason: z.string().nullable().optional(),
});

export type Signal = z.infer<typeof SignalSchema>;
