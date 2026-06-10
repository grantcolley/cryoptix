import type { Module } from "@/routing/module";
import GenericError from "@/features/errors/generic-error";
import StrategyPage from "@/pages/strategy-page";

export const MODULE_CONFIG: Module[] = [
  {
    moduleId: 1,
    name: "Strategies",
    icon: "chartArea",
    permission: "strategies.access",
    categories: [
      {
        categoryId: 1,
        name: "Moving Averages",
        icon: "chartCandlestick",
        permission: "moving-averages.access",
        pages: [
          {
            routeId: 1,
            path: "Strategy",
            element: StrategyPage,
            errorElement: GenericError,
            args: "",
            name: "Strategy",
            icon: "chartLine",
            permission: "config.access",
          },
        ],
      },
    ],
  },
];
