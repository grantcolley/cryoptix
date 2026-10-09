import { z } from "zod";
import { IndicatorType } from "./indicator-type";
import { IndicatorDefinitions } from "./indicator-definitions";
import { IndicatorValueSchema } from "./indicator-value-schema";
import { IndicatorValueTypeLabels } from "./indicator-value-type";

export const IndicatorSchema = z
  .object({
    id: z.string().uuid(),
    name: z.string().nullable().optional(),
    values: z.array(IndicatorValueSchema),
    indicatorType: z.enum(IndicatorType).default(IndicatorType.Sma),
  })
  .superRefine((indicator, ctx) => {
    const definitions = IndicatorDefinitions[indicator.indicatorType].values;
    const requiredTypes = new Set(definitions.map(({ type }) => type));
    const seen = new Set<number>();

    indicator.values.forEach(({ type }, index) => {
      if (seen.has(type)) {
        ctx.addIssue({
          code: "custom",
          path: ["values", index, "type"],
          message: `Duplicate ${IndicatorValueTypeLabels[type]} value`,
        });
      }
      if (!requiredTypes.has(type)) {
        ctx.addIssue({
          code: "custom",
          path: ["values", index, "type"],
          message: `Unexpected ${IndicatorValueTypeLabels[type]} value`,
        });
      } else if (definitions[index]?.type !== type) {
        ctx.addIssue({
          code: "custom",
          path: ["values", index, "type"],
          message: "Values must follow the indicator definition order",
        });
      }
      seen.add(type);
    });

    definitions.forEach(({ type }) => {
      if (!seen.has(type)) {
        ctx.addIssue({
          code: "custom",
          path: ["values"],
          message: `Missing ${IndicatorValueTypeLabels[type]} value`,
        });
      }
    });
  });

export type Indicator = z.infer<typeof IndicatorSchema>;
