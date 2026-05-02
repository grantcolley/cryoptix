import { z } from "zod";

const envSchema = z.object({
  VITE_AUTH_DOMAIN: z.string().min(1),
  VITE_AUTH_CLIENT_ID: z.string().min(1),
  VITE_AUTH_AUDIENCE: z.string().min(1),
});

const env = envSchema.parse(import.meta.env);

export const config = {
  AUTH_DOMAIN: env.VITE_AUTH_DOMAIN,
  AUTH_CLIENT_ID: env.VITE_AUTH_CLIENT_ID,
  AUTH_AUDIENCE: env.VITE_AUTH_AUDIENCE,
};
