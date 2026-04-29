import { createBrowserRouter } from "react-router-dom";
import { MODULE_CONFIG } from "./module-config";
import { mapModulesToRoutesBreadcrumbsAndNav } from "./route-mapper";
import ErrorPage from "@/features/errors/ErrorPage";
import App from "@/App.tsx";

function buildRoutesFromModules() {
  const { routes, modules } =
    mapModulesToRoutesBreadcrumbsAndNav(MODULE_CONFIG);

  return [
    {
      path: "/",
      element: <App modules={modules} />,
      errorElement: <ErrorPage />,
      children: routes,
    },
  ];
}

export const router = createBrowserRouter(buildRoutesFromModules());
