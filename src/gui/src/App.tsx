import { Outlet } from "react-router-dom";
import { useState } from "react";
import { useAuth0 } from "@auth0/auth0-react";
import { AppSidebar } from "@/features/sidebar/app-sidebar";
import { AppHeader } from "@/features/app/app-header";
import { SidebarInset, SidebarProvider } from "@/components/ui/sidebar";
import { TooltipProvider } from "@/components/ui/tooltip";
import type { Module } from "@/routing/module";
import { ContentOverlayContext } from "@/providers/content-overlay-context";

type Props = {
  modules: Module[];
};

const App = ({ modules }: Props) => {
  const { isAuthenticated } = useAuth0();
  const [contentOverlayContainer, setContentOverlayContainer] =
    useState<HTMLDivElement | null>(null);

  return (
    <TooltipProvider>
      <SidebarProvider
        key={isAuthenticated ? "authenticated" : "anonymous"}
        defaultOpen={isAuthenticated}
        style={
          {
            "--sidebar-width": "calc(var(--spacing) * 72)",
            "--header-height": "calc(var(--spacing) * 12)",
          } as React.CSSProperties
        }
      >
        <AppSidebar variant="inset" modules={modules} />

        <SidebarInset>
          <AppHeader />
          <div className="relative isolate flex flex-1 flex-col">
            <ContentOverlayContext.Provider value={contentOverlayContainer}>
              <div className="@container/main flex flex-1 flex-col gap-2">
                <Outlet />
              </div>
            </ContentOverlayContext.Provider>
            <div
              ref={setContentOverlayContainer}
              className="pointer-events-none absolute inset-0 z-50 overflow-hidden"
            />
          </div>
        </SidebarInset>
      </SidebarProvider>
    </TooltipProvider>
  );
};

export default App;
