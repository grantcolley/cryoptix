import * as React from "react";
import { Link } from "react-router-dom";
import { CryoptixLogo } from "@/components/CryoptixLogo";
import type { Module } from "@/routing/module";
import { AppSidebarContent } from "./app-sidebar-content";
import {
  Sidebar,
  SidebarContent,
  SidebarFooter,
  SidebarHeader,
  SidebarMenu,
  SidebarMenuButton,
  SidebarMenuItem,
  useSidebar,
} from "@/components/ui/sidebar";

type Props = {
  modules: Module[];
} & React.ComponentProps<typeof Sidebar>;

function SidebarLogo() {
  const { state } = useSidebar();
  const collapsed = state === "collapsed";

  return (
    <Link to="/" className="flex w-full items-center justify-center">
      <CryoptixLogo
        variant={collapsed ? "icon" : "wordmark"}
        className="h-4 w-auto max-h-[32px]"
      />
    </Link>
  );
}

function SidebarFooterText() {
  const { state } = useSidebar();
  const collapsed = state === "collapsed";

  return (
    <p
      className={
        collapsed
          ? "absolute bottom-2 left-1/2 -translate-x-1/2 text-xs text-muted-foreground writing-vertical rotate-180"
          : "text-sm text-muted-foreground flex h-full items-end justify-center"
      }
    >
      &copy; 2026 Cryoptix
    </p>
  );
}

export function AppSidebar({ modules, ...props }: Props) {
  const { state } = useSidebar();
  const collapsed = state === "collapsed";

  return (
    <Sidebar collapsible="icon" {...props}>
      <SidebarHeader>
        <SidebarMenu>
          <SidebarMenuItem>
            <SidebarMenuButton
              asChild
              className="data-[slot=sidebar-menu-button]:!p-1.5"
            >
              <SidebarLogo />
            </SidebarMenuButton>
          </SidebarMenuItem>
        </SidebarMenu>
      </SidebarHeader>

      <SidebarContent>
        <AppSidebarContent modules={modules} />
      </SidebarContent>

      <SidebarFooter className={collapsed ? "relative min-h-40" : undefined}>
        <SidebarFooterText />
      </SidebarFooter>
    </Sidebar>
  );
}
