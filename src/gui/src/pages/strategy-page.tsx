import * as React from "react";
import { useAuth0 } from "@auth0/auth0-react";
import { Config } from "@/config/config";
import { STRATEGY_CONFIG } from "@/data/strategy-config";
import type { Strategy } from "@/features/api/schema/strategy-schema";
import { StrategyToolbar } from "@/features/strategy/strategy-toolbar";
import { StrategySelect } from "@/features/strategy/strategy-select";
import { IndicatorSnapshot } from "@/features/strategy/indicator-snapshot";
import { MacdSnapshot } from "@/features/strategy/macd-snapshot";
import { createSignalRConnection } from "@/signalr/signalRConnection";
import type { MarketDataSnapshot } from "@/features/api/messages/market-data-snapshot-schema";
import type { Indicators } from "@/features/api/schema/indicators-schema";
import type { Kline } from "@/features/api/schema/kline-schema";
import type { Signal } from "@/features/api/schema/signal-schema";
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
import { useSidebar } from "@/components/ui/sidebar";
import type { Symbol as ApiSymbol } from "@/features/api/schema/symbol-schema";

import {
  KlineChart,
  MacdChart,
  createKlineChartController,
  type KlineChartController,
} from "@/features/charts";

type PriceDirection = "up" | "down" | "flat";

export function StrategyPage() {
  const { getAccessTokenSilently } = useAuth0();
  const { isMobile, open, openMobile, setOpen, setOpenMobile } = useSidebar();

  const [isStrategyConfigOpen, setIsStrategyConfigOpen] = React.useState(false);
  const [showParametersOnly, setShowParametersOnly] = React.useState(false);
  const [showDisconnectButton, setShowDisconnectButton] = React.useState(false);
  const [selectedStrategyId, setSelectedStrategyId] = React.useState("");
  const [strategyFormVersion, setStrategyFormVersion] = React.useState(0);
  const [showStartButton, setShowStartButton] = React.useState(false);
  const [showChart, setShowChart] = React.useState(false);
  const [chartController] = React.useState<KlineChartController>(
    createKlineChartController
  );
  const [chartRightScaleWidth, setChartRightScaleWidth] = React.useState(0);
  const [macdIndicators, setMacdIndicators] = React.useState<Indicators[]>([]);
  const macdIndicatorsRef = React.useRef<Map<number, Indicators>>(new Map());
  const [serverUrl, setServerUrl] = React.useState("");
  const [isConnecting, setIsConnecting] = React.useState(false);
  const [strategyStatus, setStrategyStatus] =
    React.useState<StrategyStatus | null>(null);
  const [editedStrategy, setEditedStrategy] = React.useState<Strategy | null>(
    null
  );
  const [connectError, setConnectError] = React.useState<string | null>(null);
  const [notificationMessage, setNotificationMessage] = React.useState<
    string | null
  >(null);
  const [price, setPrice] = React.useState<string | null>(null);
  const [priceDirection, setPriceDirection] =
    React.useState<PriceDirection>("flat");
  const [symbol, setSymbol] = React.useState<ApiSymbol | null>(null);
  const [symbolName, setSymbolName] = React.useState<string | null>(null);
  const [indicatorSnapshots, setIndicatorSnapshots] = React.useState<
    Indicators["snapshots"]
  >([]);
  const indicatorSnapshotsRef = React.useRef<Map<string, number>>(new Map());
  const [latestMacds, setLatestMacds] = React.useState<
    Array<{ key: string; name: string; value: number; signal: number; histogram: number }>
  >([]);
  const latestMacdsRef = React.useRef<
    Map<string, { name: string; value: number; signal: number; histogram: number }>
  >(new Map());

  const latestStrategyRef = React.useRef<Strategy | null>(null);
  const previousPriceRef = React.useRef<number | null>(null);
  const notificationConnectionRef = React.useRef<ReturnType<
    typeof createSignalRConnection
  > | null>(null);

  const selectedStrategy =
    STRATEGY_CONFIG.find((s) => String(s.strategyId) === selectedStrategyId) ??
    null;

  const sourceStrategy = strategyStatus?.strategy ?? selectedStrategy;
  const strategy = editedStrategy ?? sourceStrategy;

  const strategyState = strategyStatus?.strategyState;

  const showStrategyRunning = strategyState === 2;
  const showConnectButton =
    !showStartButton && !showStrategyRunning && !showDisconnectButton;
  const isStrategyConfigActive = isStrategyConfigOpen && !showParametersOnly;
  const isStrategyParametersActive = isStrategyConfigOpen && showParametersOnly;
  const hasSymbol = symbol !== null;
  const valuePrecision = Math.min(
    100,
    Math.max(0, symbol?.baseAssetPrecision ?? 0)
  );
  const priceClassName =
    priceDirection === "down"
      ? "text-destructive"
      : priceDirection === "up"
        ? "text-green-600 dark:text-green-400"
        : "text-foreground";
  const valueWidthCh = Math.max(14, valuePrecision + 10);

  const resetChartData = () => {
    chartController.reset();
    macdIndicatorsRef.current.clear();
    setMacdIndicators([]);
    indicatorSnapshotsRef.current = new Map();
    setIndicatorSnapshots([]);
    latestMacdsRef.current.clear();
    setLatestMacds([]);
    setShowChart(false);
  };
  const retainMacdIndicators = (indicators: Indicators[]) => {
    for (const indicator of indicators) {
      if (Object.keys(indicator.macds).length > 0) {
        macdIndicatorsRef.current.set(
          indicator.timestampUtc.getTime(),
          indicator
        );
      }
    }
    setMacdIndicators(
      [...macdIndicatorsRef.current.entries()]
        .sort(([a], [b]) => a - b)
        .map(([, indicator]) => indicator)
    );
  };
  const applySymbol = (nextSymbol: ApiSymbol | null) => {
    setSymbol(nextSymbol);
    setSymbolName(nextSymbol?.name ?? nextSymbol?.exchangeSymbol ?? null);
  };

  const resetPriceComparison = () => {
    previousPriceRef.current = null;
    setPrice(null);
    setPriceDirection("flat");
  };

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

      if (nextStatus?.strategyState === 2) {
        setIsStrategyConfigOpen(false);
        setShowParametersOnly(false);
      }
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

  const retainIndicatorSnapshots = (indicators: Indicators[]) => {
    for (const indicator of indicators) {
      for (const { key, value } of indicator.snapshots) {
        indicatorSnapshotsRef.current.set(key, value);
      }
    }
  };

  const applyIndicatorSnapshots = () => {
    setIndicatorSnapshots(
      [...indicatorSnapshotsRef.current.entries()]
        .sort(([a], [b]) => a.localeCompare(b))
        .map(([key, value]) => ({ key, value }))
    );
  };

  const retainLatestMacds = (indicators: Indicators[]) => {
    for (const indicator of indicators) {
      for (const [key, macd] of Object.entries(indicator.macds)) {
        latestMacdsRef.current.set(key, {
          name: macd.name ?? key,
          value: macd.value,
          signal: macd.signal,
          histogram: macd.histogram,
        });
      }
    }
    setLatestMacds(
      [...latestMacdsRef.current.entries()]
        .sort(([a], [b]) => a.localeCompare(b))
        .map(([key, macd]) => ({ key, ...macd }))
    );
  };

  const handleNotification = (envelope: NotificationEnvelope) => {
    switch (envelope.messageType) {
      case MessageType.MarketDataSnapshot: {
        const payload = envelope.payload as MarketDataSnapshot | undefined;

        if (payload) {
          applyRunningStrategyStatus(payload.strategy, null);
          applySymbol(payload.symbol);
          chartController.replaceSnapshot(payload);
          macdIndicatorsRef.current.clear();
          retainMacdIndicators(payload.indicators);
          latestMacdsRef.current.clear();
          retainLatestMacds(
            [...payload.indicators].sort(
              (a, b) => a.timestampUtc.getTime() - b.timestampUtc.getTime()
            )
          );
          if (payload.klines.length > 0) {
            setShowChart(true);
          }
          retainIndicatorSnapshots(
            [...payload.indicators].sort(
              (a, b) => a.timestampUtc.getTime() - b.timestampUtc.getTime()
            )
          );
          applyIndicatorSnapshots();

          setNotificationMessage(null);
        }

        break;
      }
      case MessageType.Kline: {
        const payload = envelope.payload as Kline | undefined;

        if (payload) {
          chartController.applyKline(payload);
          applyIndicatorSnapshots();
          setShowChart(true);
        }

        setNotificationMessage(null);
        break;
      }
      case MessageType.Indicator: {
        const payload = envelope.payload as Indicators | undefined;

        if (payload) {
          chartController.applyIndicator(payload);
          retainMacdIndicators([payload]);
          retainLatestMacds([payload]);
          retainIndicatorSnapshots([payload]);
        }

        setNotificationMessage(null);
        break;
      }
      case MessageType.Signal: {
        const payload = envelope.payload as Signal | undefined;

        if (payload) {
          chartController.applySignal(payload);
        }

        setNotificationMessage(null);
        break;
      }
      case MessageType.Trade: {
        const payload = envelope.payload as Trade | undefined;

        if (payload) {
          const nextPrice = payload.price;
          const previousPrice = previousPriceRef.current;

          setPrice(`${nextPrice}`);
          setPriceDirection(
            previousPrice === null || nextPrice === previousPrice
              ? "flat"
              : nextPrice > previousPrice
                ? "up"
                : "down"
          );
          previousPriceRef.current = nextPrice;
        }

        setNotificationMessage(null);
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

          setNotificationMessage(null);
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
        setNotificationMessage(null);
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
      return parsedStatus;
    }

    await stopSignalRSubscription();
    setNotificationMessage(null);
    return parsedStatus;
  };

  const closeSidebar = () => {
    if (isMobile && openMobile) {
      setOpenMobile(false);
    } else if (!isMobile && open) {
      setOpen(false);
    }
  };

  const handleConnect = async () => {
    setIsConnecting(true);
    setConnectError(null);
    setStrategyStatus(null);
    setShowStartButton(false);
    setEditedStrategy(null);
    setShowParametersOnly(false);
    resetChartData();
    applySymbol(null);
    latestStrategyRef.current = null;
    resetPriceComparison();

    try {
      const accessToken = await getAccessTokenSilently();

      const status = await fetchStrategyStatus(accessToken);

      if (status.strategyState === StrategyState.Running) {
        closeSidebar();
      }
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
    setIsStrategyConfigOpen(false);
    setShowParametersOnly(false);
    resetPriceComparison();
    setNotificationMessage(null);
    applySymbol(null);
    resetChartData();
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
    isStart = false,
    isUpdate = false,
    isStop = false
  ) => {
    setIsConnecting(true);
    setConnectError(null);

    try {
      const accessToken = await getAccessTokenSilently();

      if (isStart) {
        closeSidebar();
        setIsStrategyConfigOpen(false);
        setShowParametersOnly(false);
        setShowStartButton(false);
        resetChartData();
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
    setIsStrategyConfigOpen(Boolean(nextStrategyId));
    setShowParametersOnly(false);
    resetChartData();
    applySymbol(null);
    resetPriceComparison();
  };

  const handleStrategyConfigOpenChange = (open: boolean) => {
    setIsStrategyConfigOpen(open);

    if (!open) {
      setShowParametersOnly(false);
    }
  };

  const handleToggleStrategyConfig = () => {
    const shouldOpenConfig = !isStrategyConfigActive;

    setShowParametersOnly(false);
    setIsStrategyConfigOpen(shouldOpenConfig);
  };

  const handleToggleStrategyParameters = () => {
    const shouldOpenParameters = !isStrategyParametersActive;

    setShowParametersOnly(shouldOpenParameters);
    setIsStrategyConfigOpen(shouldOpenParameters);
  };

  const handleStrategyFormChange = React.useCallback(
    (nextStrategy: Strategy) => {
      latestStrategyRef.current = nextStrategy;
      setEditedStrategy(nextStrategy);
    },
    [setEditedStrategy]
  );

  return (
    <div className="flex h-[calc(100svh-var(--header-height))] min-h-0 flex-1 flex-col p-2 md:h-[calc(100svh-var(--header-height)-1rem)]">
      <div className="flex min-h-0 flex-1 flex-col overflow-hidden rounded-xl bg-muted/50">
        <StrategyToolbar
          connectError={connectError}
          notificationMessage={notificationMessage}
          onConnectSubmit={handleServerConnectSubmit}
          executionProps={{
            isConnecting,
            showConnectButton,
            showStartButton,
            showUpdateAndStopButtons: showStrategyRunning,
            showDisconnectButton,
            serverUrl,
            strategy,
            onServerUrlChange: setServerUrl,
            onStart: () => {
              const strategyToSend = latestStrategyRef.current ?? strategy;
              if (!strategyToSend) return;
              void handleStrategyAction(
                Config.API_ROUTE_START,
                strategyToSend,
                true,
                false,
                false
              );
            },
            onDisconnect: () => {
              clearCurrentStrategy();
            },
            onUpdate: () => {
              const strategyToSend = latestStrategyRef.current ?? strategy;
              if (!strategyToSend) return;
              void handleStrategyAction(
                Config.API_ROUTE_UPDATE,
                strategyToSend,
                false,
                true,
                false
              );
            },
            onStop: () => {
              void handleStrategyAction(
                Config.API_ROUTE_STOP,
                undefined,
                false,
                false,
                true
              );
            },
          }}
          headerProps={{
            showStrategyRunning,
            strategy,
            exchange: symbol?.exchange ?? null,
            isStrategyParametersActive,
            isStrategyConfigActive,
            onToggleStrategyParameters: handleToggleStrategyParameters,
            onToggleStrategyConfig: handleToggleStrategyConfig,
          }}
        />

        <StrategySelect
          canSelectStrategy={showStartButton}
          showParametersOnly={showParametersOnly}
          isOpen={isStrategyConfigOpen}
          selectedStrategyId={selectedStrategyId}
          strategy={strategy}
          strategyFormVersion={strategyFormVersion}
          onOpenChange={handleStrategyConfigOpenChange}
          onStrategyChange={handleStrategyChange}
          onStrategyFormChange={handleStrategyFormChange}
        />

        {showStrategyRunning ? (
          <div className="flex min-h-0 flex-1 rounded-xl px-4 py-2">
            <Card className="flex min-h-0 flex-1 flex-col">
              <CardHeader>
                <CardTitle className="flex min-h-5 flex-wrap items-baseline justify-start gap-1 text-left">
                  {hasSymbol ? (
                    <>
                      <h4 className="text-sm text-foreground-semimuted">
                        {symbolName}
                      </h4>
                    </>
                  ) : null}
                  {price ? (
                    <p
                      className={`px-4 text-left text-sm tabular-nums ${priceClassName}`}
                      style={{ width: `${valueWidthCh}ch` }}
                    >
                      {price}
                    </p>
                  ) : null}
                  <dl
                    className="ml-auto flex flex-wrap items-baseline justify-end gap-x-4 gap-y-1 text-left text-sm font-normal"
                    aria-label="Indicator snapshots"
                    style={{ marginRight: chartRightScaleWidth }}
                  >
                    {latestMacds.map(({ key, ...props }) => (
                      <MacdSnapshot key={key} {...props} />
                    ))}
                    {indicatorSnapshots.map(({ key, value }) => (
                      <IndicatorSnapshot key={key} name={key} value={value} />
                    ))}
                  </dl>
                </CardTitle>
              </CardHeader>
              <CardContent className="flex min-h-0 flex-1 flex-col">
                {showChart ? (
                  <>
                    <KlineChart
                      controller={chartController}
                      indicators={strategy?.indicators}
                      priceLabel={symbolName ?? "Price"}
                      onRightScaleWidthChange={setChartRightScaleWidth}
                    />
                    <MacdChart indicators={macdIndicators} />
                    <a
                      href="https://www.tradingview.com/"
                      target="_blank"
                      rel="noreferrer"
                      className="mt-1 text-right text-[10px] leading-none text-muted-foreground hover:text-foreground"
                    >
                      Charting by TradingView
                    </a>
                  </>
                ) : (
                  <div
                    className="flex min-h-0 flex-1 items-center justify-center"
                    aria-label="Waiting for chart data"
                    role="status"
                  >
                    <div className="h-6 w-6 animate-spin rounded-full border-2 border-muted-foreground border-t-transparent" />
                  </div>
                )}
              </CardContent>
            </Card>
          </div>
        ) : null}
      </div>
    </div>
  );
}

export default StrategyPage;
