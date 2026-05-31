import * as React from "react";
import { useAuth0 } from "@auth0/auth0-react";
import {
  CandlestickSeries,
  ColorType,
  CrosshairMode,
  LineSeries,
  createChart,
  createSeriesMarkers,
  type CandlestickData,
  type IChartApi,
  type ISeriesMarkersPluginApi,
  type ISeriesApi,
  type LineData,
  type SeriesMarker,
  type Time,
  type UTCTimestamp,
} from "lightweight-charts";
import { Config } from "@/config/config";
import { STRATEGY_CONFIG } from "@/data/strategy-config";
import type { Strategy } from "@/features/api/schema/strategy-schema";
import { StrategyExecution } from "@/features/strategy/strategy-execution";
import { StrategySelect } from "@/features/strategy/strategy-select";
import { createSignalRConnection } from "@/signalr/signalRConnection";
import type { MarketDataSnapshot } from "@/features/api/messages/market-data-snapshot-schema";
import type { Indicators } from "@/features/api/schema/indicators-schema";
import type { Kline } from "@/features/api/schema/kline-schema";
import { SignalType } from "@/features/api/schema/signal-type";
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
import { Button } from "@/components/ui/button";
import {
  Tooltip,
  TooltipContent,
  TooltipTrigger,
} from "@/components/ui/tooltip";
import { icons } from "@/components/icon/icons";
import { Icon } from "@/components/icon/icon";
import { ExchangeLabels } from "@/features/api/schema/exchange";
import type { Symbol as ApiSymbol } from "@/features/api/schema/symbol-schema";

type PriceDirection = "up" | "down" | "flat";
type IndicatorSeriesData = {
  key: string;
  data: LineData<UTCTimestamp>[];
};
type IndicatorLatestValues = Record<string, number>;
type IndicatorValueDirections = Record<string, PriceDirection>;

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

export function StrategyPage() {
  const { getAccessTokenSilently } = useAuth0();

  const [isStrategyConfigOpen, setIsStrategyConfigOpen] = React.useState(false);
  const [showParametersOnly, setShowParametersOnly] = React.useState(false);
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
  const [notificationMessage, setNotificationMessage] = React.useState<
    string | null
  >(null);
  const [price, setPrice] = React.useState<string | null>(null);
  const [priceDirection, setPriceDirection] =
    React.useState<PriceDirection>("flat");
  const [indicatorLatestValues, setIndicatorLatestValues] =
    React.useState<IndicatorLatestValues>({});
  const [indicatorValueDirections, setIndicatorValueDirections] =
    React.useState<IndicatorValueDirections>({});
  const [symbol, setSymbol] = React.useState<ApiSymbol | null>(null);
  const [symbolName, setSymbolName] = React.useState<string | null>(null);
  const [symbolExchange, setSymbolExchange] = React.useState<
    ApiSymbol["exchange"] | null
  >(null);

  const latestStrategyRef = React.useRef<Strategy | null>(null);
  const previousPriceRef = React.useRef<number | null>(null);
  const chartRef = React.useRef<HTMLDivElement | null>(null);
  const chartApiRef = React.useRef<IChartApi | null>(null);
  const candleSeriesRef = React.useRef<ISeriesApi<"Candlestick"> | null>(null);
  const signalMarkersRef = React.useRef<ISeriesMarkersPluginApi<Time> | null>(
    null
  );
  const signalMarkersDataRef = React.useRef<SeriesMarker<Time>[]>([]);
  const indicatorSeriesRefs = React.useRef<ISeriesApi<"Line">[]>([]);
  const indicatorSeriesDataRef = React.useRef<IndicatorSeriesData[]>([]);
  const indicatorLatestValuesRef = React.useRef<IndicatorLatestValues>({});
  const candleDataByTimeRef = React.useRef<
    Map<number, CandlestickData<UTCTimestamp>>
  >(new Map());
  const marketDataSnapshotCountRef = React.useRef(0);
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
  const getValueDirectionClassName = (direction: PriceDirection) =>
    direction === "down"
      ? "text-destructive"
      : direction === "up"
        ? "text-green-600 dark:text-green-400"
        : "";
  const hasSymbol = symbol !== null;
  const valuePrecision = Math.min(
    100,
    Math.max(0, symbol?.baseAssetPrecision ?? 0)
  );
  const valueWidthCh = Math.max(14, valuePrecision + 10);

  const applySymbol = (nextSymbol: ApiSymbol | null) => {
    setSymbol(nextSymbol);
    setSymbolName(nextSymbol?.name ?? nextSymbol?.exchangeSymbol ?? null);
    setSymbolExchange(nextSymbol?.exchange ?? null);
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

  const toChartTime = (date: Date): UTCTimestamp =>
    Math.floor(date.getTime() / 1000) as UTCTimestamp;

  const toCandleData = (kline: Kline): CandlestickData<UTCTimestamp> => ({
    time: toChartTime(kline.openTime),
    open: kline.open,
    high: kline.high,
    low: kline.low,
    close: kline.close,
  });

  const sortByTime = React.useCallback(function sortByTime<
    T extends { time: UTCTimestamp },
  >(items: T[]) {
    return items.sort((a, b) => a.time - b.time);
  }, []);

  const toIndicatorSeriesData = (
    indicators: Indicators[]
  ): IndicatorSeriesData[] => {
    const seriesByKey = new Map<string, Map<number, LineData<UTCTimestamp>>>();

    for (const indicator of indicators) {
      const time = toChartTime(indicator.timestampUtc);

      for (const item of indicator.values) {
        const dataByTime =
          seriesByKey.get(item.key) ??
          new Map<number, LineData<UTCTimestamp>>();

        dataByTime.set(time, {
          time,
          value: item.value,
        });
        seriesByKey.set(item.key, dataByTime);
      }
    }

    return [...seriesByKey.entries()].map(([key, dataByTime]) => ({
      key,
      data: sortByTime([...dataByTime.values()]),
    }));
  };

  const toIndicatorLatestValues = (
    indicators: Indicators[]
  ): IndicatorLatestValues => {
    const latestByKey = new Map<string, { time: number; value: number }>();

    for (const indicator of indicators) {
      const time = indicator.timestampUtc.getTime();

      for (const item of indicator.values) {
        const current = latestByKey.get(item.key);

        if (!current || time >= current.time) {
          latestByKey.set(item.key, {
            time,
            value: item.value,
          });
        }
      }
    }

    return Object.fromEntries(
      [...latestByKey.entries()].map(([key, latest]) => [key, latest.value])
    );
  };

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

  const toSignalMarkers = (signals: Signal[]): SeriesMarker<Time>[] => {
    const markers = signals
      .filter((signal) => signal.signalType !== SignalType.None)
      .map<SeriesMarker<Time>>((signal) => ({
        time: toChartTime(signal.timestampUtc),
        position: "belowBar",
        shape: "arrowUp",
        color: "#16a34a",
        text: signal.reason ?? undefined,
      }));

    return markers.sort((a, b) => Number(a.time) - Number(b.time));
  };

  const applySignalMarkersToChart = React.useCallback(() => {
    const candleSeries = candleSeriesRef.current;

    if (!candleSeries) {
      return;
    }

    const signalMarkers =
      signalMarkersRef.current ?? createSeriesMarkers(candleSeries);

    signalMarkersRef.current = signalMarkers;
    signalMarkers.setMarkers(signalMarkersDataRef.current);
  }, []);

  const clearIndicatorSeries = React.useCallback(() => {
    const chart = chartApiRef.current;

    if (chart) {
      for (const indicatorSeries of indicatorSeriesRefs.current) {
        chart.removeSeries(indicatorSeries);
      }
    }

    indicatorSeriesRefs.current = [];
  }, []);

  const addIndicatorSeriesToChart = React.useCallback(() => {
    const chart = chartApiRef.current;

    if (!chart) {
      return;
    }

    clearIndicatorSeries();

    indicatorSeriesRefs.current = indicatorSeriesDataRef.current.map(
      ({ key, data }, index) => {
        const indicatorSeries = chart.addSeries(LineSeries, {
          title: key,
          color: getIndicatorSeriesColor(index),
          lineWidth: 2,
          priceLineVisible: false,
          lastValueVisible: false,
          priceScaleId: "left",
        });

        indicatorSeries.setData(data);

        return indicatorSeries;
      }
    );
  }, [clearIndicatorSeries]);

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
      candleSeries.setData(
        sortByTime([...candleDataByTimeRef.current.values()])
      );
      chartApiRef.current?.timeScale().fitContent();
    }
  };

  const applyIndicatorsToChart = (indicators: Indicators[]) => {
    const latestValues = toIndicatorLatestValues(indicators);

    indicatorSeriesDataRef.current = toIndicatorSeriesData(indicators);
    indicatorLatestValuesRef.current = latestValues;
    setIndicatorLatestValues(latestValues);
    setIndicatorValueDirections({});
    addIndicatorSeriesToChart();
    chartApiRef.current?.timeScale().fitContent();
  };

  const applyIndicatorToChart = (indicator: Indicators) => {
    const time = toChartTime(indicator.timestampUtc);
    const seriesByKey = new Map(
      indicatorSeriesDataRef.current.map(({ key, data }) => [key, data])
    );

    for (const item of indicator.values) {
      const data = seriesByKey.get(item.key) ?? [];
      const nextData = data.filter((point) => point.time !== time);

      nextData.push({
        time,
        value: item.value,
      });

      seriesByKey.set(item.key, sortByTime(nextData));
    }

    const previousLatestValues = indicatorLatestValuesRef.current;
    const updatedValueKeys = new Set(indicator.values.map((item) => item.key));
    const nextLatestValues = {
      ...previousLatestValues,
      ...Object.fromEntries(
        indicator.values.map((item) => [item.key, item.value])
      ),
    };

    indicatorLatestValuesRef.current = nextLatestValues;
    setIndicatorLatestValues(nextLatestValues);
    setIndicatorValueDirections(
      Object.fromEntries(
        Object.entries(nextLatestValues).map(([key, value]) => {
          const previousValue = previousLatestValues[key];
          const direction =
            !updatedValueKeys.has(key) ||
            previousValue === undefined ||
            value === previousValue
              ? "flat"
              : value > previousValue
                ? "up"
                : "down";

          return [key, direction];
        })
      )
    );

    indicatorSeriesDataRef.current = [...seriesByKey.entries()].map(
      ([key, data]) => ({
        key,
        data,
      })
    );
    addIndicatorSeriesToChart();
  };

  const applySignalsToChart = (signals: Signal[]) => {
    signalMarkersDataRef.current = toSignalMarkers(signals);
    applySignalMarkersToChart();
  };

  const applySignalToChart = (signal: Signal) => {
    signalMarkersDataRef.current = [
      ...signalMarkersDataRef.current,
      ...toSignalMarkers([signal]),
    ].sort((a, b) => Number(a.time) - Number(b.time));
    applySignalMarkersToChart();
  };

  const resetChartData = () => {
    candleDataByTimeRef.current = new Map();
    indicatorSeriesDataRef.current = [];
    signalMarkersDataRef.current = [];
    indicatorLatestValuesRef.current = {};
    setIndicatorLatestValues({});
    setIndicatorValueDirections({});
    candleSeriesRef.current?.setData([]);
    signalMarkersRef.current?.setMarkers([]);
    clearIndicatorSeries();
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

  const handleNotification = (envelope: NotificationEnvelope) => {
    switch (envelope.messageType) {
      case MessageType.MarketDataSnapshot: {
        const payload = envelope.payload as MarketDataSnapshot | undefined;

        if (payload) {
          const marketDataSnapshotCount =
            (marketDataSnapshotCountRef.current += 1);
          const snapshotTime =
            payload?.snapshotTimeUtc instanceof Date
              ? payload.snapshotTimeUtc.toISOString()
              : undefined;

          const message = snapshotTime
            ? `Market data snapshot received at ${snapshotTime}.`
            : "Market data snapshot received.";

          applyRunningStrategyStatus(payload.strategy, message);
          applySymbol(payload.symbol);
          applyKlinesToChart(payload.klines, true);
          applyIndicatorsToChart(payload.indicators);
          applySignalsToChart(payload.signals);

          setNotificationMessage(
            payload.symbol.name
              ? `Market data snapshot #${marketDataSnapshotCount} for ${payload.symbol.name} received. ${payload.klines.length} klines, ${payload.indicators.length} indicators, and ${payload.signals.length} signals included. Symbol precision is ${payload.symbol.baseAssetPrecision}.`
              : `Market data snapshot #${marketDataSnapshotCount} received.`
          );
        }

        break;
      }
      case MessageType.Kline: {
        const payload = envelope.payload as Kline | undefined;

        if (payload) {
          applyKlinesToChart([payload]);
        }

        // setNotificationMessage(null);
        break;
      }
      case MessageType.Indicator: {
        const payload = envelope.payload as Indicators | undefined;

        if (payload) {
          applyIndicatorToChart(payload);
        }

        // setNotificationMessage(null);
        break;
      }
      case MessageType.Signal: {
        const payload = envelope.payload as Signal | undefined;

        if (payload) {
          applySignalToChart(payload);
        }

        // setNotificationMessage(null);
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

        // setNotificationMessage(null);
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
    setShowParametersOnly(false);
    setIndicatorLatestValues({});
    indicatorLatestValuesRef.current = {};
    setIndicatorValueDirections({});
    applySymbol(null);
    latestStrategyRef.current = null;
    resetPriceComparison();

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

  React.useEffect(() => {
    const container = chartRef.current;

    if (!showStrategyRunning || !container) {
      return;
    }

    const chart = createChart(container, {
      autoSize: true,
      layout: {
        attributionLogo: false,
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
      leftPriceScale: {
        borderColor: "#cbd5e1",
        visible: true,
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
    applySignalMarkersToChart();
    addIndicatorSeriesToChart();
    chart.timeScale().fitContent();

    return () => {
      signalMarkersRef.current?.detach();
      chartApiRef.current = null;
      candleSeriesRef.current = null;
      signalMarkersRef.current = null;
      indicatorSeriesRefs.current = [];
      chart.remove();
    };
  }, [
    addIndicatorSeriesToChart,
    applySignalMarkersToChart,
    showStrategyRunning,
    sortByTime,
    symbolName,
  ]);

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
        setIsStrategyConfigOpen(false);
        setShowParametersOnly(false);
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
    setIsStrategyConfigOpen(Boolean(nextStrategyId));
    setShowParametersOnly(false);
    setIndicatorLatestValues({});
    indicatorLatestValuesRef.current = {};
    setIndicatorValueDirections({});
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
    },
    []
  );

  const strategyPeriods = React.useMemo(
    () => (strategy ? Object.entries(strategy.periods) : []),
    [strategy]
  );

  return (
    <div className="flex h-[calc(100svh-var(--header-height))] min-h-0 flex-1 flex-col p-2 md:h-[calc(100svh-var(--header-height)-1rem)]">
      <div className="flex min-h-0 flex-1 flex-col overflow-hidden rounded-xl bg-muted/50">
        <form
          className="flex items-center gap-1 px-4 pt-4 pb-2"
          onSubmit={handleServerConnectSubmit}
        >
          <StrategyExecution
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
              {strategyPeriods.length > 0 ? (
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
              ) : null}
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
                    onClick={handleToggleStrategyParameters}
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
                    onClick={handleToggleStrategyConfig}
                  >
                    <Icon
                      icon={
                        isStrategyConfigActive ? icons.minimize2 : icons.cog
                      }
                    />
                  </Button>
                </TooltipTrigger>
                <TooltipContent>{strategyConfigTooltip}</TooltipContent>
              </Tooltip>
            </div>
          </div>
        ) : null}

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
                <CardTitle>{hasSymbol ? symbolName : null}</CardTitle>
              </CardHeader>
              <CardContent className="flex min-h-0 flex-1 flex-col">
                <div ref={chartRef} className="min-h-0 flex-1 w-full" />
                <a
                  href="https://www.tradingview.com/"
                  target="_blank"
                  rel="noreferrer"
                  className="mt-1 text-left text-[10px] leading-none text-muted-foreground hover:text-foreground"
                >
                  Charting by TradingView
                </a>
              </CardContent>
            </Card>
          </div>
        ) : null}
      </div>
    </div>
  );
}

export default StrategyPage;
