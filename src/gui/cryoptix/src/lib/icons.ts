import {
  Check,
  ChevronRight,
  ChartCandlestick,
  ChartLine,
  CircleHelp, // general fallback
  FileQuestion, // missing page icon
  Folder, // missing category icon
  Boxes, // missing module icon
  Moon,
  Sun,
  type LucideProps,
  ChartArea,
} from "lucide-react";
import type { ComponentType } from "react";

/**
 * Central icon registry
 */
export const icons = {
  check: Check,
  chartArea: ChartArea,
  chartCandlestick: ChartCandlestick,
  chartLine: ChartLine,
  circleHelp: CircleHelp,
  fileQuestion: FileQuestion,
  folder: Folder,
  boxes: Boxes,
  moon: Moon,
  sun: Sun,
  cheveronRight: ChevronRight,
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
