import { useTheme } from "@/providers/theme-provider";
import { Icon } from "@/components/icon/icon";
import { icons } from "@/components/icon/icons";
import { Button } from "@/components/ui/button";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import {
  Tooltip,
  TooltipContent,
  TooltipProvider,
  TooltipTrigger,
} from "@/components/ui/tooltip";

export function ThemeToggle() {
  const { theme, setTheme } = useTheme();

  return (
    <TooltipProvider>
      <DropdownMenu>
        <Tooltip>
          <TooltipTrigger asChild>
            <DropdownMenuTrigger asChild>
              <Button variant="outline" size="icon">
                <Icon
                  icon={icons.sun}
                  className="h-[1.2rem] w-[1.2rem] scale-100 rotate-0 transition-all dark:scale-0 dark:-rotate-90"
                  aria-label="Light mode"
                />
                <Icon
                  icon={icons.moon}
                  className="absolute h-[1.2rem] w-[1.2rem] scale-0 rotate-90 transition-all dark:scale-100 dark:rotate-0"
                  aria-label="Dark mode"
                />
                <span className="sr-only">Toggle theme</span>
              </Button>
            </DropdownMenuTrigger>
          </TooltipTrigger>
          <TooltipContent>Toggle theme</TooltipContent>
        </Tooltip>

        <DropdownMenuContent align="end">
          <DropdownMenuItem onClick={() => setTheme("light")}>
            <span className="mr-2 inline-flex w-4">
              {theme === "light" ? <Icon icon={icons.check} /> : null}
            </span>
            Light
          </DropdownMenuItem>

          <DropdownMenuItem onClick={() => setTheme("dark")}>
            <span className="mr-2 inline-flex w-4">
              {theme === "dark" ? <Icon icon={icons.check} /> : null}
            </span>
            Dark
          </DropdownMenuItem>

          <DropdownMenuItem onClick={() => setTheme("system")}>
            <span className="mr-2 inline-flex w-4">
              {theme === "system" ? <Icon icon={icons.check} /> : null}
            </span>
            System
          </DropdownMenuItem>
        </DropdownMenuContent>
      </DropdownMenu>
    </TooltipProvider>
  );
}
