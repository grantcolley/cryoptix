import { createBrowserRouter } from "react-router-dom";
import { MODULE_CONFIG } from "./module-config";
import { mapModulesToRoutesBreadcrumbsAndNav } from "./route-mapper";
import { HomePage } from "@/pages/home-page";
import { ErrorBoundary } from "react-error-boundary";
import ErrorPage from "@/features/errors/ErrorPage";
import { RequireAuth } from "@/features/auth/require-auth";
import App from "@/App.tsx";

function buildRoutesFromModules() {
  const { routes, modules } =
    mapModulesToRoutesBreadcrumbsAndNav(MODULE_CONFIG);

  return [
    {
      path: "/",
      element: (
        <ErrorBoundary FallbackComponent={ErrorPage}>
          <App modules={modules} />
        </ErrorBoundary>
      ),
      errorElement: <ErrorPage />,
      children: [
        {
          index: true,
          element: <HomePage />,
        },
        ...routes.map((route) => ({
          ...route,
          element: <RequireAuth>{route.element}</RequireAuth>,
        })),
      ],
    },
  ];
}

export const router = createBrowserRouter(buildRoutesFromModules());
