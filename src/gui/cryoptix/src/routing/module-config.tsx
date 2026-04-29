import type { Module } from "./module";
import Strategy from "@/features/strategy/strategy";
import GenericError from "@/features/errors/generic-error";

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
            element: Strategy,
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
