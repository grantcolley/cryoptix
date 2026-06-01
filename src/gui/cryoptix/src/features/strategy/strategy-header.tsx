import { Icon } from "@/components/icon/icon";
import { icons } from "@/components/icon/icons";
import { Button } from "@/components/ui/button";
import {
  Tooltip,
  TooltipContent,
  TooltipTrigger,
} from "@/components/ui/tooltip";
import { ExchangeLabels } from "@/features/api/schema/exchange";
import type { Symbol as ApiSymbol } from "@/features/api/schema/symbol-schema";
import type { Strategy } from "@/features/api/schema/strategy-schema";

type PriceDirection = "up" | "down" | "flat";
type IndicatorLatestValues = Record<string, number>;
type IndicatorValueDirections = Record<string, PriceDirection>;

interface StrategyHeaderProps {
  connectError: string | null;
  notificationMessage: string | null;
  showStrategyRunning: boolean;
  strategy: Strategy | null;
  hasSymbol: boolean;
  symbolExchange: ApiSymbol["exchange"] | null;
  symbolName: string | null;
  price: string | null;
  priceDirection: PriceDirection;
  valuePrecision: number;
  strategyPeriods: [string, number][];
  indicatorLatestValues: IndicatorLatestValues;
  indicatorValueDirections: IndicatorValueDirections;
  isStrategyParametersActive: boolean;
  isStrategyConfigActive: boolean;
  onToggleStrategyParameters: () => void;
  onToggleStrategyConfig: () => void;
}

const INDICATOR_SERIES_COLORS = [
  "#2563eb",
  "#f97316",
  "#7c3aed",
  "#0891b2",
  "#db2777",
  "#65a30d",
];

const getIndicatorSeriesColor = (index: number) =>
  INDICATOR_SERIES_COLORS[index % INDICATOR_SERIES_COLORS.length];

const getValueDirectionClassName = (direction: PriceDirection) =>
  direction === "down"
    ? "text-destructive"
    : direction === "up"
      ? "text-green-600 dark:text-green-400"
      : "";

export function StrategyHeader({
  connectError,
  notificationMessage,
  showStrategyRunning,
  strategy,
  hasSymbol,
  symbolExchange,
  symbolName,
  price,
  priceDirection,
  valuePrecision,
  strategyPeriods,
  indicatorLatestValues,
  indicatorValueDirections,
  isStrategyParametersActive,
  isStrategyConfigActive,
  onToggleStrategyParameters,
  onToggleStrategyConfig,
}: StrategyHeaderProps) {
  const strategyConfigTooltip = isStrategyConfigActive
    ? "Hide strategy config"
    : "Show strategy config";
  const strategyParametersTooltip = isStrategyParametersActive
    ? "Hide strategy parameters"
    : "Show strategy parameters";
  const priceClassName =
    priceDirection === "down"
      ? "text-destructive"
      : priceDirection === "up"
        ? "text-green-600 dark:text-green-400"
        : "text-foreground";
  const valueWidthCh = Math.max(14, valuePrecision + 10);

  const formatDisplayValue = (value: number | string | undefined): string => {
    if (value === undefined || value === null) {
      return "--";
    }

    const numericValue = Number(value);

    if (!Number.isFinite(numericValue)) {
      return String(value);
    }

    return numericValue.toFixed(valuePrecision);
  };

  return (
    <>
      {connectError && (
        <p className="px-4 text-sm text-destructive">{connectError}</p>
      )}

      {notificationMessage && (
        <p className="px-4 text-sm text-muted-foreground">
          {notificationMessage}
        </p>
      )}

      {showStrategyRunning && strategy ? (
        <div className="flex flex-row items-baseline gap-4 px-4 py-1">
          <div className="flex items-center gap-1">
            {hasSymbol ? (
              <>
                <h4 className="text-sm text-foreground-semimuted mr-2">
                  {symbolExchange == null
                    ? null
                    : ExchangeLabels[symbolExchange]}
                </h4>
                <h4 className="text-sm text-foreground-semimuted">
                  {symbolName}
                </h4>
              </>
            ) : null}
            {price && (
              <p
                className={`px-4 text-left text-sm tabular-nums ${priceClassName}`}
                style={{ width: `${valueWidthCh}ch` }}
              >
                {formatDisplayValue(price)}
              </p>
            )}
            {/* {strategyPeriods.length > 0 ? (
              <div className="flex flex-wrap items-center gap-1">
                {strategyPeriods.map(([key], index) => (
                  <span
                    key={key}
                    className="inline-flex items-center gap-1 rounded px-2 py-0.5 text-xs font-medium leading-5 text-foreground shadow-sm"
                  >
                    <span style={{ color: getIndicatorSeriesColor(index) }}>
                      {key}:
                    </span>
                    <span
                      className={`text-left tabular-nums ${getValueDirectionClassName(
                        indicatorValueDirections[key] ?? "flat"
                      )}`}
                      style={{ width: `${valueWidthCh}ch` }}
                    >
                      {formatDisplayValue(indicatorLatestValues[key])}
                    </span>
                  </span>
                ))}
              </div>
            ) : null} */}
          </div>
          <div className="ml-auto flex items-center gap-1">
            <h4 className="text-sm text-foreground-semimuted mr-2">
              {strategy.name}
            </h4>
            <Tooltip>
              <TooltipTrigger asChild>
                <Button
                  id="btnStrategyParameters"
                  variant="outline"
                  size="icon"
                  aria-label={strategyParametersTooltip}
                  onClick={onToggleStrategyParameters}
                >
                  <Icon
                    icon={
                      isStrategyParametersActive
                        ? icons.minimize2
                        : icons.slidersHorizontal
                    }
                  />
                </Button>
              </TooltipTrigger>
              <TooltipContent>{strategyParametersTooltip}</TooltipContent>
            </Tooltip>
            <Tooltip>
              <TooltipTrigger asChild>
                <Button
                  id="btnStrategyConfig"
                  variant="outline"
                  size="icon"
                  aria-label={strategyConfigTooltip}
                  onClick={onToggleStrategyConfig}
                >
                  <Icon
                    icon={isStrategyConfigActive ? icons.minimize2 : icons.cog}
                  />
                </Button>
              </TooltipTrigger>
              <TooltipContent>{strategyConfigTooltip}</TooltipContent>
            </Tooltip>
          </div>
        </div>
      ) : null}
    </>
  );
}
