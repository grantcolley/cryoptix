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
