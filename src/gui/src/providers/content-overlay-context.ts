import { createContext } from "react";

// Overlay portals share the content area's bounds below the application header.
export const ContentOverlayContext = createContext<HTMLElement | null>(null);
