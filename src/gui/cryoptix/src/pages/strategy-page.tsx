import * as React from "react";
import { useAuth0 } from "@auth0/auth0-react";
import { Config } from "@/config/config";
import { ExchangeLabels } from "@/features/strategy/schema/exchange";
// import  { type Strategy } from "@/features/strategy/schema/strategy-schema";
import StrategyForm from "@/features/strategy/strategy-form";
import { STRATEGY_CONFIG } from "@/data/strategy-config";
import { Icon } from "@/components/icon/icon";
import { icons } from "@/components/icon/icons";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import {
  StrategyStatusSchema,
  type StrategyStatus,
} from "@/features/strategy/schema/strategy-status";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { Collapsible, CollapsibleContent } from "@/components/ui/collapsible";
import {
  Tooltip,
  TooltipContent,
  TooltipProvider,
  TooltipTrigger,
} from "@/components/ui/tooltip";

export function StrategyPage() {
  const { getAccessTokenSilently } = useAuth0();
  const [isOpen, setIsOpen] = React.useState(false);
  const [selectedStrategyId, setSelectedStrategyId] = React.useState("");

  const [serverUrl, setServerUrl] = React.useState("");
  const [isConnecting, setIsConnecting] = React.useState(false);
  const [strategyStatus, setStrategyStatus] =
    React.useState<StrategyStatus | null>(null);
  const [connectError, setConnectError] = React.useState<string | null>(null);

  const selectedStrategy =
    STRATEGY_CONFIG.find((s) => String(s.strategyId) === selectedStrategyId) ??
    null;

  const strategy = strategyStatus?.strategy ?? selectedStrategy;

  const strategyState = strategyStatus?.strategyState;

  const showStartButton = strategyState === 0;
  const showUpdateAndStopButtons = strategyState === 2;
  const showConnectButton = !showStartButton && !showUpdateAndStopButtons;

  const getStrategyStatusUrl = (baseUrl: string) => {
    const normalizedBaseUrl = baseUrl.endsWith("/") ? baseUrl : `${baseUrl}/`;
    return new URL(Config.API_ROUTE_STATUS, normalizedBaseUrl).toString();
  };

  const handleConnect = async () => {
    setIsConnecting(true);
    setConnectError(null);
    setStrategyStatus(null);

    try {
      const accessToken = await getAccessTokenSilently();

      const response = await fetch(getStrategyStatusUrl(serverUrl), {
        method: "GET",
        headers: {
          Authorization: `Bearer ${accessToken}`,
        },
      });

      if (!response.ok) {
        throw new Error(`Request failed with status ${response.status}`);
      }

      const json: unknown = await response.json();

      const parsedStatus = StrategyStatusSchema.parse(json);

      setStrategyStatus(parsedStatus);

      if (parsedStatus.strategy) {
        setSelectedStrategyId(String(parsedStatus.strategy.strategyId));
        setIsOpen(true);
      }
    } catch (error) {
      setConnectError(getErrorMessage(error));
    } finally {
      setIsConnecting(false);
    }
  };

  const getErrorMessage = (error: unknown): string => {
    if (error instanceof TypeError && error.message === "Failed to fetch") {
      return "Failed to connect to server";
    }

    if (error instanceof Error) {
      return error.message;
    }

    return "An unexpected error occurred";
  };

  const handleStrategyChange = (value: string) => {
    const nextStrategyId = value === "__none__" ? "" : value;

    setSelectedStrategyId(nextStrategyId);
    setIsOpen(Boolean(nextStrategyId));
  };

  return (
    <div className="flex flex-1 flex-col p-2">
      <div className="flex-1 rounded-xl bg-muted/50 md:min-h-min">
        <div className="flex items-center gap-1 px-4 pt-4 pb-2">
          <Input
            id="server-url"
            type="text"
            placeholder="Server url..."
            aria-label="Enter server url"
            value={serverUrl}
            onChange={(event) => setServerUrl(event.target.value)}
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
                      variant="outline"
                      size="icon"
                      aria-label="Connect to server"
                      onClick={() => {
                        void handleConnect();
                      }}
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
                      variant="outline"
                      size="icon"
                      aria-label="Start strategy"
                      onClick={() => {
                        void handleConnect();
                      }}
                      disabled={!serverUrl.trim()}
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
                        variant="outline"
                        size="icon"
                        aria-label="Update strategy parameters"
                        onClick={() => {
                          void handleConnect();
                        }}
                        disabled={!serverUrl.trim()}
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
                        variant="outline"
                        size="icon"
                        aria-label="Stop strategy"
                        onClick={() => {
                          void handleConnect();
                        }}
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
        </div>

        {connectError && (
          <p className="px-4 text-sm text-destructive">{connectError}</p>
        )}

        {strategyStatus?.message && (
          <p className="px-4 text-sm text-muted-foreground">
            {strategyStatus.message}
          </p>
        )}

        <Collapsible
          open={isOpen}
          onOpenChange={setIsOpen}
          className="group/collapsible grid auto-rows-min rounded-xl px-4 py-2"
        >
          <div className="flex items-center gap-1 py-2">
            {showStartButton && (
              <Select
                value={selectedStrategyId}
                onValueChange={handleStrategyChange}
                aria-label="Select a strategy"
              >
                <SelectTrigger className="w-[255px]">
                  <SelectValue placeholder="Select a strategy" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="__none__">No strategy</SelectItem>
                  {STRATEGY_CONFIG.map((s) => (
                    <SelectItem key={s.strategyId} value={String(s.strategyId)}>
                      {s.name}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            )}

            {strategy && showUpdateAndStopButtons && (
              <div className="flex flex-row items-baseline gap-4">
                <h4 className="text-md text-foreground">{strategy.name}</h4>
                <h4 className="text-sm text-foreground-semimuted">
                  {strategy.symbol}
                </h4>
                <h4 className="text-sm text-foreground-semimuted">
                  {ExchangeLabels[strategy.exchange]}
                </h4>
              </div>
            )}
          </div>

          {strategy && (
            <CollapsibleContent className="flex flex-col gap-2 pt-2 pb-2">
              <StrategyForm defaultValues={strategy} showSubmitButton={false} />
            </CollapsibleContent>
          )}
        </Collapsible>

        <div className="min-h-[100vh] flex-1 rounded-xl md:min-h-min px-4 py-2">
          Live Charts...
        </div>
      </div>
    </div>
  );
}

export default StrategyPage;
