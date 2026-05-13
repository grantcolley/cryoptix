import * as React from "react";
import { useAuth0 } from "@auth0/auth0-react";
import { Config } from "@/config/config";
import { STRATEGY_CONFIG } from "@/data/strategy-config";
import { ExchangeLabels } from "@/features/api/schema/exchange";
import type { Strategy } from "@/features/api/schema/strategy-schema";
import StrategyForm from "@/features/strategy/strategy-form";
import { StrategyHeader } from "@/features/strategy/strategy-header";
import { createSignalRConnection } from "@/signalr/signalRConnection";
import type { MarketDataSnapshot } from "@/features/api/messages/market-data-snapshot-schema";
import type { Trade } from "@/features/api/schema/trade-schema";
import type { NotificationEnvelope } from "@/features/api/messages/notification-envelope-schema";
import { NotificationEnvelopeSchema } from "@/features/api/messages/notification-envelope-schema";
import { MessageType } from "@/features/api/messages/message-type";
import {
  StrategyStatusSchema,
  type StrategyStatus,
} from "@/features/api/schema/strategy-status";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { Collapsible, CollapsibleContent } from "@/components/ui/collapsible";

export function StrategyPage() {
  const { getAccessTokenSilently } = useAuth0();

  const [isOpen, setIsOpen] = React.useState(false);
  const [selectedStrategyId, setSelectedStrategyId] = React.useState("");
  const [strategyFormVersion, setStrategyFormVersion] = React.useState(0);

  const [serverUrl, setServerUrl] = React.useState("");
  const [isConnecting, setIsConnecting] = React.useState(false);
  const [strategyStatus, setStrategyStatus] =
    React.useState<StrategyStatus | null>(null);
  const [editedStrategy, setEditedStrategy] = React.useState<Strategy | null>(
    null
  );
  const [connectError, setConnectError] = React.useState<string | null>(null);

  const latestStrategyRef = React.useRef<Strategy | null>(null);
  const notificationConnectionRef = React.useRef<ReturnType<
    typeof createSignalRConnection
  > | null>(null);
  const [notificationMessage, setNotificationMessage] = React.useState<
    string | null
  >(null);

  const selectedStrategy =
    STRATEGY_CONFIG.find((s) => String(s.strategyId) === selectedStrategyId) ??
    null;

  const sourceStrategy = strategyStatus?.strategy ?? selectedStrategy;
  const strategy = editedStrategy ?? sourceStrategy;

  const strategyState = strategyStatus?.strategyState;

  const showStartButton = strategyState === 0;
  const showUpdateAndStopButtons = strategyState === 2;
  const showConnectButton = !showStartButton && !showUpdateAndStopButtons;
  const showLiveCharts = strategyState === 2 && Boolean(strategy);

  const getApiUrl = (baseUrl: string, route: string) => {
    const normalizedBaseUrl = baseUrl.endsWith("/") ? baseUrl : `${baseUrl}/`;
    return new URL(route, normalizedBaseUrl).toString();
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

  const applyStrategyStatus = (nextStatus: StrategyStatus) => {
    setStrategyStatus(nextStatus);

    if (nextStatus.strategy) {
      latestStrategyRef.current = nextStatus.strategy;
      setEditedStrategy(nextStatus.strategy);
      setSelectedStrategyId(String(nextStatus.strategy.strategyId));
      setStrategyFormVersion((version) => version + 1);
      setIsOpen(true);
    }
  };

  const stopSignalRSubscription = React.useCallback(async () => {
    const connection = notificationConnectionRef.current;

    if (!connection) {
      return;
    }

    notificationConnectionRef.current = null;

    try {
      await connection.stop();
    } catch (error) {
      console.warn("Failed to stop SignalR subscription", error);
    }
  }, []);

  const handleNotification = (envelope: NotificationEnvelope) => {
    switch (envelope.messageType) {
      case MessageType.MarketDataSnapshot: {
        const payload = envelope.payload as MarketDataSnapshot | undefined;
        const snapshotTime =
          payload?.snapshotTimeUtc instanceof Date
            ? payload.snapshotTimeUtc.toISOString()
            : undefined;

        setNotificationMessage(
          snapshotTime
            ? `Market data snapshot received at ${snapshotTime}.`
            : "Market data snapshot received."
        );
        break;
      }
      case MessageType.Kline:
        setNotificationMessage("Kline update received.");
        break;
      case MessageType.Trade: {
        const payload = envelope.payload as Trade | undefined;
        const price = payload?.price;

        setNotificationMessage(
          price !== undefined
            ? `Trade update received at ${price}.`
            : "Trade update received."
        );
        break;
      }
      case MessageType.StrategyUpdated:
        setNotificationMessage("Strategy update received.");
        break;
      case MessageType.None:
      default:
        setNotificationMessage("No notifications available.");
        break;
    }
  };

  const startSignalRSubscription = async (accessToken: string) => {
    await stopSignalRSubscription();

    if (!serverUrl.trim()) {
      return;
    }

    const connection = createSignalRConnection(
      serverUrl,
      Config.API_ROUTE_SUBSCRIBE,
      () => accessToken
    );

    connection.on("ReceiveNotification", (message: unknown) => {
      const parsed = NotificationEnvelopeSchema.safeParse(message);
      if (!parsed.success) {
        console.warn("Invalid notification envelope", parsed.error);
        return;
      }

      handleNotification(parsed.data);
    });

    notificationConnectionRef.current = connection;

    try {
      await connection.start();
    } catch (error) {
      notificationConnectionRef.current = null;
      const errorMessage =
        error instanceof Error ? error.message : String(error);
      setConnectError(`SignalR connection failed: ${errorMessage}`);
    }
  };

  const fetchStrategyStatus = async (accessToken: string) => {
    const response = await fetch(
      getApiUrl(serverUrl, Config.API_ROUTE_STATUS),
      {
        method: "GET",
        headers: {
          Authorization: `Bearer ${accessToken}`,
        },
      }
    );

    if (!response.ok) {
      throw new Error(`Request failed with status ${response.status}`);
    }

    const json: unknown = await response.json();
    const parsedStatus = StrategyStatusSchema.parse(json);

    applyStrategyStatus(parsedStatus);

    if (parsedStatus.strategyState === 2) {
      await startSignalRSubscription(accessToken);
      return;
    }

    await stopSignalRSubscription();
    setNotificationMessage(null);
  };

  const handleConnect = async () => {
    setIsConnecting(true);
    setConnectError(null);
    setStrategyStatus(null);
    setEditedStrategy(null);
    latestStrategyRef.current = null;

    try {
      const accessToken = await getAccessTokenSilently();

      await fetchStrategyStatus(accessToken);
    } catch (error) {
      setConnectError(getErrorMessage(error));
    } finally {
      setIsConnecting(false);
    }
  };

  const clearCurrentStrategy = () => {
    latestStrategyRef.current = null;
    setEditedStrategy(null);
    setStrategyStatus(null);
    setSelectedStrategyId("");
    setIsOpen(false);
    setNotificationMessage(null);
    void stopSignalRSubscription();
  };

  React.useEffect(() => {
    return () => {
      void stopSignalRSubscription();
    };
  }, [stopSignalRSubscription]);

  const handleStrategyAction = async (
    route: string,
    strategyBody?: Strategy,
    clearStrategyAfterSuccess = false
  ) => {
    setIsConnecting(true);
    setConnectError(null);

    try {
      const accessToken = await getAccessTokenSilently();

      const response = await fetch(getApiUrl(serverUrl, route), {
        method: "POST",
        headers: {
          Authorization: `Bearer ${accessToken}`,
          ...(strategyBody ? { "Content-Type": "application/json" } : {}),
        },
        body: strategyBody ? JSON.stringify(strategyBody) : undefined,
      });

      if (!response.ok) {
        throw new Error(`Request failed with status ${response.status}`);
      }

      if (clearStrategyAfterSuccess) {
        clearCurrentStrategy();
        return;
      }

      await fetchStrategyStatus(accessToken);
    } catch (error) {
      setConnectError(getErrorMessage(error));
    } finally {
      setIsConnecting(false);
    }
  };

  const handleServerConnectSubmit = (
    event: React.SyntheticEvent<HTMLFormElement>
  ) => {
    event.preventDefault();

    if (!showConnectButton || isConnecting || !serverUrl.trim()) {
      return;
    }

    void handleConnect();
  };

  const handleStrategyChange = (value: string) => {
    const nextStrategyId = value === "__none__" ? "" : value;

    const nextStrategy =
      STRATEGY_CONFIG.find((s) => String(s.strategyId) === nextStrategyId) ??
      null;

    latestStrategyRef.current = nextStrategy;
    setSelectedStrategyId(nextStrategyId);
    setEditedStrategy(nextStrategy);
    setStrategyFormVersion((version) => version + 1);
    setIsOpen(Boolean(nextStrategyId));
  };

  const handleStrategyFormChange = React.useCallback(
    (nextStrategy: Strategy) => {
      latestStrategyRef.current = nextStrategy;
    },
    []
  );

  return (
    <div className="flex flex-1 flex-col p-2">
      <div className="flex-1 rounded-xl bg-muted/50 md:min-h-min">
        <form
          className="flex items-center gap-1 px-4 pt-4 pb-2"
          onSubmit={handleServerConnectSubmit}
        >
          <StrategyHeader
            isConnecting={isConnecting}
            showConnectButton={showConnectButton}
            showStartButton={showStartButton}
            showUpdateAndStopButtons={showUpdateAndStopButtons}
            serverUrl={serverUrl}
            strategy={strategy}
            onServerUrlChange={setServerUrl}
            onStart={() => {
              const strategyToSend = latestStrategyRef.current ?? strategy;
              if (!strategyToSend) return;
              void handleStrategyAction(Config.API_ROUTE_START, strategyToSend);
            }}
            onUpdate={() => {
              const strategyToSend = latestStrategyRef.current ?? strategy;
              if (!strategyToSend) return;
              void handleStrategyAction(Config.API_ROUTE_UPDATE, strategyToSend);
            }}
            onStop={() => {
              void handleStrategyAction(Config.API_ROUTE_STOP, undefined, true);
            }}
          />
        </form>

        {connectError && (
          <p className="px-4 text-sm text-destructive">{connectError}</p>
        )}

        {strategyStatus?.message && (
          <p className="px-4 text-sm text-muted-foreground">
            {strategyStatus.message}
          </p>
        )}

        {notificationMessage && (
          <p className="px-4 text-sm text-muted-foreground">
            {notificationMessage}
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
              <StrategyForm
                key={`${strategy.strategyId}-${strategyFormVersion}`}
                defaultValues={strategy}
                showSubmitButton={false}
                onChange={handleStrategyFormChange}
              />
            </CollapsibleContent>
          )}
        </Collapsible>

        {showLiveCharts ? (
          <div className="min-h-[100vh] flex-1 rounded-xl md:min-h-min px-4 py-2">
            Live Charts...
          </div>
        ) : null}
      </div>
    </div>
  );
}

export default StrategyPage;
