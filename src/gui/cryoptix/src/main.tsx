import { StrictMode } from "react";
import { createRoot } from "react-dom/client";
import { RouterProvider } from "react-router-dom";
import { ThemeProvider } from "./providers/theme-provider.tsx";
import { router } from "@/routing/router";
import "@/components/CryoptixLogo.css";
import "./index.css";

createRoot(document.getElementById("root")!).render(
  <StrictMode>
    <ThemeProvider defaultTheme="system" storageKey="cryoptix-theme">
      <RouterProvider router={router} />
    </ThemeProvider>
  </StrictMode>
);
