import { z } from "zod";

export const SignalType = {
  None: 0,
  Buy: 1,
  Sell: 2,
  ExitLong: 3,
  ExitShort: 4,
} as const;

export const SignalTypeSchema = z.enum(SignalType);

export type SignalType = z.infer<typeof SignalTypeSchema>;

export const SignalTypeLabels: Record<SignalType, string> = {
  [SignalType.None]: "None",
  [SignalType.Buy]: "Buy",
  [SignalType.Sell]: "Sell",
  [SignalType.ExitLong]: "Exit Long",
  [SignalType.ExitShort]: "Exit Short",
};
