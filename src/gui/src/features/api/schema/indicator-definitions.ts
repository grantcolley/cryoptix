import { IndicatorType } from "./indicator-type";
import { IndicatorValueType } from "./indicator-value-type";
import type { IndicatorValue } from "./indicator-value-schema";

export type IndicatorValueDefinition = {
  type: IndicatorValueType;
  defaultValue: number;
};

export type IndicatorDefinition = {
  values: readonly IndicatorValueDefinition[];
};

export const IndicatorDefinitions: Record<IndicatorType, IndicatorDefinition> =
  {
    [IndicatorType.None]: { values: [] },
    [IndicatorType.Sma]: {
      values: [{ type: IndicatorValueType.Period, defaultValue: 9 }],
    },
    [IndicatorType.Ema]: {
      values: [{ type: IndicatorValueType.Period, defaultValue: 9 }],
    },
    [IndicatorType.Rsi]: {
      values: [{ type: IndicatorValueType.Period, defaultValue: 14 }],
    },
    [IndicatorType.Macd]: {
      values: [
        { type: IndicatorValueType.FastPeriod, defaultValue: 12 },
        { type: IndicatorValueType.SlowPeriod, defaultValue: 26 },
        { type: IndicatorValueType.SignalPeriod, defaultValue: 9 },
      ],
    },
  };

export function getDefaultIndicatorValues(
  indicatorType: IndicatorType
): IndicatorValue[] {
  return IndicatorDefinitions[indicatorType].values.map(
    ({ type, defaultValue }) => ({ type, value: defaultValue })
  );
}
