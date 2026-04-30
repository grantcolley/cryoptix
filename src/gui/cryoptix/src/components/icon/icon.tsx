import { cn } from "@/lib/utils";
import type { LucideIcon } from "lucide-react";

type IconSize = "sm" | "md" | "lg";

const sizes: Record<IconSize, string> = {
  sm: "h-4 w-4",
  md: "h-5 w-5",
  lg: "h-6 w-6",
};

type IconProps = {
  icon: LucideIcon;
  size?: IconSize;
  className?: string;
};

export function Icon({
  icon: IconComponent,
  size = "md",
  className,
}: IconProps) {
  return (
    <IconComponent
      className={cn(sizes[size], "text-muted-foreground", className)}
    />
  );
}
