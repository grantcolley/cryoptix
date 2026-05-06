import { z } from "zod";

const envSchema = z.object({
  VITE_AUTH_DOMAIN: z.string().min(1),
  VITE_AUTH_CLIENT_ID: z.string().min(1),
  VITE_AUTH_AUDIENCE: z.string().min(1),
  VITE_API_ROUTE_STATUS: z.string().min(1),
  VITE_API_ROUTE_START: z.string().min(1),
  VITE_API_ROUTE_STOP: z.string().min(1),
  VITE_API_ROUTE_UPDATE: z.string().min(1),
});

const env = envSchema.parse(import.meta.env);

export const Config = {
  AUTH_DOMAIN: env.VITE_AUTH_DOMAIN,
  AUTH_CLIENT_ID: env.VITE_AUTH_CLIENT_ID,
  AUTH_AUDIENCE: env.VITE_AUTH_AUDIENCE,
  API_ROUTE_STATUS: env.VITE_API_ROUTE_STATUS,
  API_ROUTE_START: env.VITE_API_ROUTE_START,
  API_ROUTE_STOP: env.VITE_API_ROUTE_STOP,
  API_ROUTE_UPDATE: env.VITE_API_ROUTE_UPDATE,
};
