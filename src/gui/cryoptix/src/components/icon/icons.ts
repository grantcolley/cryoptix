import {
  Boxes, // missing module icon
  ChartArea,
  ChartCandlestick,
  ChartLine,
  Check,
  ChevronRight,
  CircleHelp, // general fallback
  FileQuestion, // missing page icon
  Folder, // missing category icon
  LogIn,
  LogOut,
  Moon,
  Sun,
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
  fileQuestion: FileQuestion,
  folder: Folder,
  logIn: LogIn,
  logOut: LogOut,
  moon: Moon,
  sun: Sun,
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
