import { z } from "zod";

export const KlineInterval = {
  Unknown: 0,
  Minute: 1,
  Minutes3: 2,
  Minutes5: 3,
  Minutes15: 4,
  Minutes30: 5,
  Hour: 6,
  Hours2: 7,
  Hours4: 8,
  Hours6: 9,
  Hours8: 10,
  Hours12: 11,
  Day: 12,
  Days3: 13,
  Week: 14,
  Month: 15,
} as const;

export const KlineIntervalSchema = z.enum(KlineInterval);

export type KlineInterval = z.infer<typeof KlineIntervalSchema>;

export const KlineIntervalLabels: Record<KlineInterval, string> = {
  [KlineInterval.Unknown]: "Unknown",
  [KlineInterval.Minute]: "1 Minute",
  [KlineInterval.Minutes3]: "3 Minutes",
  [KlineInterval.Minutes5]: "5 Minutes",
  [KlineInterval.Minutes15]: "15 Minutes",
  [KlineInterval.Minutes30]: "30 Minutes",
  [KlineInterval.Hour]: "1 Hour",
  [KlineInterval.Hours2]: "2 Hours",
  [KlineInterval.Hours4]: "4 Hours",
  [KlineInterval.Hours6]: "6 Hours",
  [KlineInterval.Hours8]: "8 Hours",
  [KlineInterval.Hours12]: "12 Hours",
  [KlineInterval.Day]: "1 Day",
  [KlineInterval.Days3]: "3 Days",
  [KlineInterval.Week]: "1 Week",
  [KlineInterval.Month]: "1 Month",
};
