import { Link } from "react-router-dom";
import { Module } from "@/routing/module";
import { Icon } from "@/components/icon/icon";
import type { IconComponent } from "@/components/icon/icons";
import { getIcon } from "@/components/icon/icons";
import { icons } from "@/components/icon/icons";
import {
  Collapsible,
  CollapsibleContent,
  CollapsibleTrigger,
} from "@/components/ui/collapsible";
import {
  SidebarGroup,
  SidebarGroupLabel,
  SidebarMenu,
  SidebarMenuButton,
  SidebarMenuItem,
  SidebarMenuSub,
  SidebarMenuSubButton,
  SidebarMenuSubItem,
} from "@/components/ui/sidebar";

type Props = {
  modules: Module[];
};

function SidebarIcon({ icon: IconComponent }: { icon?: IconComponent }) {
  if (!IconComponent) return null;
  return <IconComponent />;
}

export function AppSidebarContent({ modules }: Props) {
  return (
    <>
      {modules.map((module) => (
        <SidebarGroup key={module.moduleId}>
          <SidebarGroupLabel>
            <SidebarIcon icon={getIcon(module.icon)} />
            <span>&nbsp;{module.name}</span>
          </SidebarGroupLabel>
          <SidebarMenu>
            {module.categories.map((category) => (
              <Collapsible
                key={category.categoryId}
                asChild
                className="group/collapsible"
              >
                <SidebarMenuItem>
                  <CollapsibleTrigger asChild>
                    <SidebarMenuButton tooltip={category.name}>
                      <SidebarIcon icon={getIcon(category.icon)} />
                      <span>{category.name}</span>
                      <Icon
                        icon={icons.cheveronRight}
                        className="ml-auto transition-transform duration-200 group-data-[state=open]/collapsible:rotate-90"
                      />
                    </SidebarMenuButton>
                  </CollapsibleTrigger>
                  <CollapsibleContent>
                    <SidebarMenuSub>
                      {category.pages?.map((page) => (
                        <SidebarMenuSubItem key={page.routeId}>
                          <SidebarMenuSubButton asChild>
                            <Link to={page.fullPath ?? "/"}>
                              <SidebarIcon icon={getIcon(page.icon)} />
                              <span>{page.name}</span>
                            </Link>
                          </SidebarMenuSubButton>
                        </SidebarMenuSubItem>
                      ))}
                    </SidebarMenuSub>
                  </CollapsibleContent>
                </SidebarMenuItem>
              </Collapsible>
            ))}
          </SidebarMenu>
        </SidebarGroup>
      ))}
    </>
  );
}
