import { StrictMode } from "react";
import { createRoot } from "react-dom/client";
import { RouterProvider } from "react-router-dom";
import { Auth0Provider } from "@auth0/auth0-react";
import { ThemeProvider } from "./providers/theme-provider.tsx";
import { ROUTES } from "@/routing/routes.ts";
import { router } from "@/routing/router";
import { Config } from "@/config/config";
import "@/components/icon/CryoptixLogo.css";
import "./index.css";

createRoot(document.getElementById("root")!).render(
  <StrictMode>
    <ThemeProvider defaultTheme="system" storageKey="cryoptix-theme">
      <Auth0Provider
        domain={Config.AUTH_DOMAIN}
        clientId={Config.AUTH_CLIENT_ID}
        authorizationParams={{
          redirect_uri: window.location.origin,
          audience: Config.AUTH_AUDIENCE || undefined,
        }}
        onRedirectCallback={(appState) => {
          void router.navigate(appState?.returnTo || ROUTES.HOME);
        }}
      >
        <RouterProvider router={router} />
      </Auth0Provider>
    </ThemeProvider>
  </StrictMode>
);
