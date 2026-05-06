import * as React from "react";
import { useAuth0 } from "@auth0/auth0-react";
import { Config } from "@/config/config";
import StrategyForm from "@/features/strategy/strategy-form";
import { STRATEGY_CONFIG } from "@/data/strategy-config";
import { Icon } from "@/components/icon/icon";
import { icons } from "@/components/icon/icons";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import {
  StrategyStatusSchema,
  type StrategyStatus,
} from "@/features/strategy/strategy-status";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import {
  Collapsible,
  CollapsibleContent,
  CollapsibleTrigger,
} from "@/components/ui/collapsible";

export function StrategyPage() {
  const { getAccessTokenSilently } = useAuth0();
  const [isOpen, setIsOpen] = React.useState(false);
  const [selectedStrategyId, setSelectedStrategyId] = React.useState("");

  const [serverUrl, setServerUrl] = React.useState("");
  const [isConnecting, setIsConnecting] = React.useState(false);
  const [strategyStatus, setStrategyStatus] =
    React.useState<StrategyStatus | null>(null);
  const [connectError, setConnectError] = React.useState<string | null>(null);

  const strategy =
    STRATEGY_CONFIG.find((s) => String(s.strategyId) === selectedStrategyId) ??
    null;

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

      console.log("Raw strategy status response:", json);

      const parsedStatus = StrategyStatusSchema.parse(json);

      setStrategyStatus(parsedStatus);
    } catch (error) {
      setConnectError(
        error instanceof Error ? error.message : "Failed to connect to server"
      );
    } finally {
      setIsConnecting(false);
    }
  };

  const handleStrategyChange = (value: string) => {
    const nextStrategyId = value === "__none__" ? "" : value;

    setSelectedStrategyId(nextStrategyId);

    if (!nextStrategyId) {
      setIsOpen(false);
    }
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
            disabled={isConnecting}
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
            <Button
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
          <div className="flex items-center gap-1">
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

            {strategy && (
              <CollapsibleTrigger asChild>
                <Button
                  variant="outline"
                  size="icon"
                  aria-label="Toggle details"
                >
                  <Icon icon={isOpen ? icons.minus : icons.plus} />
                </Button>
              </CollapsibleTrigger>
            )}
          </div>

          {strategy && (
            <CollapsibleContent className="flex flex-col gap-2">
              <StrategyForm
                defaultValues={strategy}
                submitLabel="Update strategy"
                onSubmit={(updated) => {
                  console.log(updated);
                }}
              />
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
