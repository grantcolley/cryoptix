import type { Strategy } from "@/features/api/schema/strategy-schema";
import { Icon } from "@/components/icon/icon";
import { icons } from "@/components/icon/icons";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import {
  Tooltip,
  TooltipContent,
  TooltipProvider,
  TooltipTrigger,
} from "@/components/ui/tooltip";

interface StrategyHeaderProps {
  isConnecting: boolean;
  showConnectButton: boolean;
  showStartButton: boolean;
  showUpdateAndStopButtons: boolean;
  serverUrl: string;
  strategy: Strategy | null;
  onServerUrlChange: (url: string) => void;
  onStart: () => void;
  onUpdate: () => void;
  onStop: () => void;
}

export function StrategyHeader({
  isConnecting,
  showConnectButton,
  showStartButton,
  showUpdateAndStopButtons,
  serverUrl,
  strategy,
  onServerUrlChange,
  onStart,
  onUpdate,
  onStop,
}: StrategyHeaderProps) {
  return (
    <>
      <Input
        id="server-url"
        type="text"
        placeholder="Server url..."
        aria-label="Enter server url"
        value={serverUrl}
        onChange={(event) => onServerUrlChange(event.target.value)}
        disabled={isConnecting || !showConnectButton}
      />

      {isConnecting ? (
        <div
          className="flex h-9 w-9 items-center justify-center rounded-md border"
          aria-label="Connecting to server"
          role="status"
        >
          <div className="h-4 w-4 animate-spin rounded-full border-2 border-muted-foreground border-t-transparent" />
        </div>
      ) : (
        <TooltipProvider>
          {showConnectButton ? (
            <Tooltip>
              <TooltipTrigger asChild>
                <Button
                  id="btn-connect"
                  type="submit"
                  variant="outline"
                  size="icon"
                  aria-label="Connect to server"
                  disabled={!serverUrl.trim()}
                >
                  <Icon icon={icons.plug} className="rotate-90" />
                </Button>
              </TooltipTrigger>
              <TooltipContent>Connect to server</TooltipContent>
            </Tooltip>
          ) : null}

          {showStartButton ? (
            <Tooltip>
              <TooltipTrigger asChild>
                <Button
                  id="btn-start"
                  type="button"
                  variant="outline"
                  size="icon"
                  aria-label="Start strategy"
                  onClick={onStart}
                  disabled={!serverUrl.trim() || !strategy}
                >
                  <Icon icon={icons.play} />
                </Button>
              </TooltipTrigger>
              <TooltipContent>Start strategy</TooltipContent>
            </Tooltip>
          ) : null}

          {showUpdateAndStopButtons ? (
            <>
              <Tooltip>
                <TooltipTrigger asChild>
                  <Button
                    id="btn-update"
                    type="button"
                    variant="outline"
                    size="icon"
                    aria-label="Update strategy parameters"
                    onClick={onUpdate}
                    disabled={!serverUrl.trim() || !strategy}
                  >
                    <Icon icon={icons.slidersHorizontal} />
                  </Button>
                </TooltipTrigger>
                <TooltipContent>Update strategy parameters</TooltipContent>
              </Tooltip>

              <Tooltip>
                <TooltipTrigger asChild>
                  <Button
                    id="btn-stop"
                    type="button"
                    variant="outline"
                    size="icon"
                    aria-label="Stop strategy"
                    onClick={onStop}
                    disabled={!serverUrl.trim()}
                  >
                    <Icon icon={icons.square} />
                  </Button>
                </TooltipTrigger>
                <TooltipContent>Stop strategy</TooltipContent>
              </Tooltip>
            </>
          ) : null}
        </TooltipProvider>
      )}
    </>
  );
}