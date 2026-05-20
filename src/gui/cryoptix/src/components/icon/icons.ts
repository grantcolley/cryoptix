import {
  Boxes, // missing module icon
  ChartArea,
  ChartCandlestick,
  ChartLine,
  Check,
  ChevronRight,
  CircleHelp, // general fallback
  Cog,
  FileQuestion, // missing page icon
  Folder, // missing category icon
  LogIn,
  LogOut,
  Minimize2,
  Minus,
  Moon,
  Play,
  Plug,
  Plus,
  SlidersHorizontal,
  Square,
  Sun,
  Unplug,
  type LucideProps,
} from "lucide-react";
import type { ComponentType } from "react";

/**
 * Central icon registry
 */
export const icons = {
  boxes: Boxes,
  chartArea: ChartArea,
  chartCandlestick: ChartCandlestick,
  chartLine: ChartLine,
  check: Check,
  cheveronRight: ChevronRight,
  circleHelp: CircleHelp,
  cog: Cog,
  fileQuestion: FileQuestion,
  folder: Folder,
  logIn: LogIn,
  logOut: LogOut,
  minimize2: Minimize2,
  minus: Minus,
  moon: Moon,
  play: Play,
  plug: Plug,
  plus: Plus,
  slidersHorizontal: SlidersHorizontal,
  square: Square,
  sun: Sun,
  unplug: Unplug,
};

/**
 * Strict union of valid icon names
 */
export type IconName = keyof typeof icons;

/**
 * Generic icon component type (Lucide-compatible)
 */
export type IconComponent = ComponentType<LucideProps>;

/**
 * Type guard for runtime safety
 */
export function isIconName(value: string): value is IconName {
  return value in icons;
}

/**
 * Safe icon resolver (handles dynamic strings)
 */
export function getIcon(name: string): IconComponent {
  if (isIconName(name)) {
    return icons[name];
  }

  // fallback icon (change if you want)
  return icons.circleHelp;
}
