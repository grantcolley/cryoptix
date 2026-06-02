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
  isStrategyParametersActive: boolean;
  isStrategyConfigActive: boolean;
  onToggleStrategyParameters: () => void;
  onToggleStrategyConfig: () => void;
}

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
                {price}
              </p>
            )}
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
