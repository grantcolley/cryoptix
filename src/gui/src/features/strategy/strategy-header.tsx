import { Icon } from "@/components/icon/icon";
import { icons } from "@/components/icon/icons";
import { Button } from "@/components/ui/button";
import {
  Tooltip,
  TooltipContent,
  TooltipTrigger,
} from "@/components/ui/tooltip";
import type { Strategy } from "@/features/api/schema/strategy-schema";

interface StrategyHeaderProps {
  connectError: string | null;
  notificationMessage: string | null;
  showStrategyRunning: boolean;
  strategy: Strategy | null;
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
            <h4 className="text-sm text-foreground-semimuted ml-2">
              {strategy.name}
            </h4>
          </div>
        </div>
      ) : null}
    </>
  );
}
