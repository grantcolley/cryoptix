import * as React from "react";
import { useAuth0 } from "@auth0/auth0-react";
import {
  CandlestickSeries,
  ColorType,
  CrosshairMode,
  createChart,
  type CandlestickData,
  type IChartApi,
  type ISeriesApi,
  type UTCTimestamp,
} from "lightweight-charts";
import { Config } from "@/config/config";
import { STRATEGY_CONFIG } from "@/data/strategy-config";
import type { Strategy } from "@/features/api/schema/strategy-schema";
import { StrategyHeader } from "@/features/strategy/strategy-header";
import { StrategySelect } from "@/features/strategy/strategy-select";
import { createSignalRConnection } from "@/signalr/signalRConnection";
import type { MarketDataSnapshot } from "@/features/api/messages/market-data-snapshot-schema";
import type { Kline } from "@/features/api/schema/kline-schema";
import type { Trade } from "@/features/api/schema/trade-schema";
import type { NotificationEnvelope } from "@/features/api/messages/notification-envelope-schema";
import { NotificationEnvelopeSchema } from "@/features/api/messages/notification-envelope-schema";
import { MessageType } from "@/features/api/messages/message-type";
import {
  StrategyStatusSchema,
  type StrategyStatus,
} from "@/features/api/schema/strategy-status";
import { StrategyState } from "@/features/api/schema/strategy-state";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";

export function StrategyPage() {
  const { getAccessTokenSilently } = useAuth0();

  const [isOpen, setIsOpen] = React.useState(false);
  const [showDisconnectButton, setShowDisconnectButton] = React.useState(false);
  const [selectedStrategyId, setSelectedStrategyId] = React.useState("");
  const [strategyFormVersion, setStrategyFormVersion] = React.useState(0);
  const [showStartButton, setShowStartButton] = React.useState(false);

  const [serverUrl, setServerUrl] = React.useState("");
  const [isConnecting, setIsConnecting] = React.useState(false);
  const [strategyStatus, setStrategyStatus] =
    React.useState<StrategyStatus | null>(null);
  const [editedStrategy, setEditedStrategy] = React.useState<Strategy | null>(
    null
  );
  const [connectError, setConnectError] = React.useState<string | null>(null);

  const latestStrategyRef = React.useRef<Strategy | null>(null);
  const chartRef = React.useRef<HTMLDivElement | null>(null);
  const chartApiRef = React.useRef<IChartApi | null>(null);
  const candleSeriesRef = React.useRef<ISeriesApi<"Candlestick"> | null>(null);
  const candleDataByTimeRef = React.useRef<
    Map<number, CandlestickData<UTCTimestamp>>
  >(new Map());
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

  const showStrategyRunning = strategyState === 2;
  const showConnectButton =
    !showStartButton && !showStrategyRunning && !showDisconnectButton;

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

  const toChartTime = (date: Date): UTCTimestamp =>
    Math.floor(date.getTime() / 1000) as UTCTimestamp;

  const toCandleData = (
    kline: Kline
  ): CandlestickData<UTCTimestamp> => ({
    time: toChartTime(kline.openTime),
    open: kline.open,
    high: kline.high,
    low: kline.low,
    close: kline.close,
  });

  const sortByTime = <T extends { time: UTCTimestamp }>(items: T[]) =>
    items.sort((a, b) => a.time - b.time);

  const applyKlinesToChart = (klines: Kline[], replace = false) => {
    const candleSeries = candleSeriesRef.current;

    if (replace) {
      candleDataByTimeRef.current = new Map();
    }

    for (const kline of klines) {
      const candle = toCandleData(kline);
      candleDataByTimeRef.current.set(candle.time, candle);

      if (!replace && candleSeries) {
        candleSeries.update(candle);
      }
    }

    if (replace && candleSeries) {
      candleSeries.setData(sortByTime([...candleDataByTimeRef.current.values()]));
      chartApiRef.current?.timeScale().fitContent();
    }
  };

  const resetChartData = () => {
    candleDataByTimeRef.current = new Map();
    candleSeriesRef.current?.setData([]);
  };

  const applyRunningStrategyStatus = (
    strategy: Strategy,
    message: string | null
  ) => {
    const nextStatus: StrategyStatus = {
      strategyState: StrategyState.Running,
      strategy: strategy,
      strategyProcessorType: strategy.strategyProcessorType,
      message: message,
    };

    applyStrategyStatus(nextStatus);
  };

  const applyStrategyStatus = (nextStatus: StrategyStatus) => {
    setStrategyStatus(nextStatus);
    setShowStartButton(nextStatus.strategyState === 0);

    if (nextStatus.strategy) {
      latestStrategyRef.current = nextStatus.strategy;
      setEditedStrategy(nextStatus.strategy);
      setSelectedStrategyId(String(nextStatus.strategy.strategyId));
      setStrategyFormVersion((version) => version + 1);
      setIsOpen(true);
    }
  };

  const stopSignalRSubscription = React.useCallback(async () => {
    setShowDisconnectButton(false);

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

        if (payload) {
          const snapshotTime =
            payload?.snapshotTimeUtc instanceof Date
              ? payload.snapshotTimeUtc.toISOString()
              : undefined;

          const message = snapshotTime
            ? `Market data snapshot received at ${snapshotTime}.`
            : "Market data snapshot received.";

          applyRunningStrategyStatus(payload.strategy, message);
          applyKlinesToChart(payload.klines, true);

          setNotificationMessage(message);
        }

        break;
      }
      case MessageType.Kline: {
        const payload = envelope.payload as Kline | undefined;

        if (payload) {
          applyKlinesToChart([payload]);
        }

        setNotificationMessage("Kline update received.");
        break;
      }
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
      case MessageType.StrategyStarted: {
        const payload = envelope.payload as Strategy | undefined;

        if (payload) {
          const envelopeTime =
            envelope?.timestampUtc instanceof Date
              ? envelope.timestampUtc.toISOString()
              : undefined;

          const message = envelopeTime
            ? `Strategy started at ${envelopeTime}.`
            : "Strategy started.";

          applyRunningStrategyStatus(payload, message);

          setNotificationMessage(message);
        }

        break;
      }
      case MessageType.StrategyUpdated: {
        const payload = envelope.payload as Strategy | undefined;

        if (payload) {
          const envelopeTime =
            envelope?.timestampUtc instanceof Date
              ? envelope.timestampUtc.toISOString()
              : undefined;

          const message = envelopeTime
            ? `Strategy updated at ${envelopeTime}.`
            : "Strategy updated.";

          applyRunningStrategyStatus(payload, message);

          setNotificationMessage(message);
        }

        break;
      }
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
      setShowDisconnectButton(true);
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
      if (!notificationConnectionRef.current) {
        await startSignalRSubscription(accessToken);
      }
      return;
    }

    await stopSignalRSubscription();
    setNotificationMessage(null);
  };

  const handleConnect = async () => {
    setIsConnecting(true);
    setConnectError(null);
    setStrategyStatus(null);
    setShowStartButton(false);
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
    setShowStartButton(false);
    setSelectedStrategyId("");
    setIsOpen(false);
    setNotificationMessage(null);
    resetChartData();
    void stopSignalRSubscription();
  };

  React.useEffect(() => {
    return () => {
      void stopSignalRSubscription();
    };
  }, [stopSignalRSubscription]);

  React.useEffect(() => {
    const container = chartRef.current;

    if (!showStrategyRunning || !container) {
      return;
    }

    const chart = createChart(container, {
      autoSize: true,
      layout: {
        background: { type: ColorType.Solid, color: "transparent" },
        textColor: "#475569",
      },
      grid: {
        vertLines: { color: "#e2e8f0" },
        horzLines: { color: "#e2e8f0" },
      },
      crosshair: {
        mode: CrosshairMode.Normal,
      },
      rightPriceScale: {
        borderColor: "#cbd5e1",
      },
      timeScale: {
        borderColor: "#cbd5e1",
        timeVisible: true,
        secondsVisible: true,
      },
    });

    const candleSeries = chart.addSeries(CandlestickSeries, {
      upColor: "#16a34a",
      downColor: "#dc2626",
      borderUpColor: "#16a34a",
      borderDownColor: "#dc2626",
      wickUpColor: "#16a34a",
      wickDownColor: "#dc2626",
    });

    chartApiRef.current = chart;
    candleSeriesRef.current = candleSeries;

    candleSeries.setData(sortByTime([...candleDataByTimeRef.current.values()]));
    chart.timeScale().fitContent();

    return () => {
      chartApiRef.current = null;
      candleSeriesRef.current = null;
      chart.remove();
    };
  }, [showStrategyRunning, strategy?.symbol]);

  const handleStrategyAction = async (
    route: string,
    strategyBody?: Strategy,
    isStart = false,
    isUpdate = false,
    isStop = false
  ) => {
    setIsConnecting(true);
    setConnectError(null);

    try {
      const accessToken = await getAccessTokenSilently();

      if (isStart) {
        setShowStartButton(false);
        await startSignalRSubscription(accessToken);
      }

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

      if (isStop) {
        clearCurrentStrategy();
        return;
      }

      if (isUpdate) {
        await fetchStrategyStatus(accessToken);
      }
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
            showUpdateAndStopButtons={showStrategyRunning}
            showDisconnectButton={showDisconnectButton}
            serverUrl={serverUrl}
            strategy={strategy}
            onServerUrlChange={setServerUrl}
            onStart={() => {
              const strategyToSend = latestStrategyRef.current ?? strategy;
              if (!strategyToSend) return;
              void handleStrategyAction(
                Config.API_ROUTE_START,
                strategyToSend,
                true,
                false,
                false
              );
            }}
            onDisconnect={() => {
              clearCurrentStrategy();
            }}
            onUpdate={() => {
              const strategyToSend = latestStrategyRef.current ?? strategy;
              if (!strategyToSend) return;
              void handleStrategyAction(
                Config.API_ROUTE_UPDATE,
                strategyToSend,
                false,
                true,
                false
              );
            }}
            onStop={() => {
              void handleStrategyAction(
                Config.API_ROUTE_STOP,
                undefined,
                false,
                false,
                true
              );
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

        <StrategySelect
          isOpen={isOpen}
          showSelect={showStartButton}
          selectedStrategyId={selectedStrategyId}
          strategy={strategy}
          strategyFormVersion={strategyFormVersion}
          onOpenChange={setIsOpen}
          onStrategyChange={handleStrategyChange}
          onStrategyFormChange={handleStrategyFormChange}
        />

        {showStrategyRunning ? (
          <div className="min-h-[100vh] flex-1 rounded-xl md:min-h-min px-4 py-2">
            <Card>
              <CardHeader>
                <CardTitle>{strategy?.symbol}</CardTitle>
              </CardHeader>
              <CardContent>
                <div ref={chartRef} className="h-[500px] w-full" />
              </CardContent>
            </Card>
          </div>
        ) : null}
      </div>
    </div>
  );
}

export default StrategyPage;
