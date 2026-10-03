import { Icon } from "@/components/icon/icon";
import { icons } from "@/components/icon/icons";
import { Button } from "@/components/ui/button";
import {
  Tooltip,
  TooltipContent,
  TooltipTrigger,
} from "@/components/ui/tooltip";
import type { Strategy } from "@/features/api/schema/strategy-schema";
import { Exchange, ExchangeLabels } from "@/features/api/schema/exchange";

interface StrategyHeaderProps {
  showStrategyRunning: boolean;
  strategy: Strategy | null;
  exchange: Exchange | null;
  isStrategyParametersActive: boolean;
  isStrategyConfigActive: boolean;
  onToggleStrategyParameters: () => void;
  onToggleStrategyConfig: () => void;
}

export function StrategyHeader({
  showStrategyRunning,
  strategy,
  exchange,
  isStrategyParametersActive,
  isStrategyConfigActive,
  onToggleStrategyParameters,
  onToggleStrategyConfig,
}: StrategyHeaderProps) {
  const exchangeLabel =
    exchange === null || exchange === Exchange.None
      ? null
      : ExchangeLabels[exchange];
  const strategyConfigTooltip = isStrategyConfigActive
    ? "Hide strategy config"
    : "Show strategy config";
  const strategyParametersTooltip = isStrategyParametersActive
    ? "Hide strategy parameters"
    : "Show strategy parameters";

  return (
    <>
      {showStrategyRunning && strategy ? (
        <div className="flex min-w-0 max-w-full flex-row items-center gap-4">
          <div className="flex min-w-0 items-center gap-1">
            <Tooltip>
              <TooltipTrigger asChild>
                <Button
                  id="btnStrategyConfig"
                  aria-haspopup="dialog"
                  aria-expanded={isStrategyConfigActive}
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
            <Tooltip>
              <TooltipTrigger asChild>
                <Button
                  id="btnStrategyParameters"
                  aria-haspopup="dialog"
                  aria-expanded={isStrategyParametersActive}
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
            <span className="ml-2 flex shrink-0 items-center gap-2">
              <span className="shrink-0">Strategy</span>
              <span className="text-right text-muted-foreground">
                {strategy.name}
              </span>
            </span>
            {exchangeLabel ? (
              <span className="ml-2 flex shrink-0 items-center gap-2">
                <span className="shrink-0">Exchange</span>
                <span className="text-right text-muted-foreground">
                  {exchangeLabel}
                </span>
              </span>
            ) : null}
          </div>
        </div>
      ) : null}
    </>
  );
}
