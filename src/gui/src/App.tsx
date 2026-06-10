import { Outlet } from "react-router-dom";
import { useAuth0 } from "@auth0/auth0-react";
import { AppSidebar } from "@/features/sidebar/app-sidebar";
import { AppHeader } from "@/features/app/app-header";
import { SidebarInset, SidebarProvider } from "@/components/ui/sidebar";
import { TooltipProvider } from "@/components/ui/tooltip";
import type { Module } from "@/routing/module";

type Props = {
  modules: Module[];
};

const App = ({ modules }: Props) => {
  const { isAuthenticated } = useAuth0();

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
          <div className="flex flex-1 flex-col">
            <div className="@container/main flex flex-1 flex-col gap-2">
              <Outlet />
            </div>
          </div>
        </SidebarInset>
      </SidebarProvider>
    </TooltipProvider>
  );
};

export default App;
