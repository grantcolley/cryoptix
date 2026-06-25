import * as React from "react";
import { useAuth0 } from "@auth0/auth0-react";
import {
  CandlestickSeries,
  ColorType,
  CrosshairMode,
  HistogramSeries,
  LineSeries,
  createChart,
  createSeriesMarkers,
  type CandlestickData,
  type HistogramData,
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
import { StrategyHeader } from "@/features/strategy/strategy-header";
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
import { Exchange, ExchangeLabels } from "@/features/api/schema/exchange";
import { MovingAverageSmoothingTypeLabels } from "@/features/api/schema/moving-average-smothing-type";
import {
  StrategyStatusSchema,
  type StrategyStatus,
} from "@/features/api/schema/strategy-status";
import { StrategyState } from "@/features/api/schema/strategy-state";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Checkbox } from "@/components/ui/checkbox";
import type { Symbol as ApiSymbol } from "@/features/api/schema/symbol-schema";

type PriceDirection = "up" | "down" | "flat";
type IndicatorSeriesData = {
  key: string;
  data: LineData<UTCTimestamp>[];
};
type ChartSeriesVisibility = Record<string, boolean>;
type ChartSeriesLabelPosition = {
  key: string;
  label: string;
  color: string;
  x: number;
  y: number;
};

const PRICE_SERIES_KEY = "price";
const VOLUME_SERIES_KEY = "volume";
const PRICE_SERIES_UP_COLOR = "#16a34a";
const PRICE_SERIES_DOWN_COLOR = "#dc2626";
const PRICE_SERIES_COLOR = PRICE_SERIES_UP_COLOR;
const VOLUME_SERIES_UP_COLOR = "rgba(22, 163, 74, 0.28)";
const VOLUME_SERIES_DOWN_COLOR = "rgba(220, 38, 38, 0.28)";
const VOLUME_SERIES_COLOR = "rgba(71, 85, 105, 0.32)";
const INITIAL_VISIBLE_KLINE_LIMIT = 120;
const MIN_INITIAL_BAR_SPACING = 3;
const MAX_INITIAL_BAR_SPACING = 12;
const MANUAL_SCROLL_THRESHOLD = 0.5;
const MOVING_AVERAGE_LABEL_PATTERN = /^\d+\s+[a-z]+$/i;

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

const formatMovingAveragePeriodLabel = (
  period: Strategy["periods"][string]
) => {
  const generatedLabel = `${period.value} ${MovingAverageSmoothingTypeLabels[period.smoothingType]}`;
  const configuredName = period.name?.trim();

  if (
    configuredName &&
    configuredName !== generatedLabel &&
    !MOVING_AVERAGE_LABEL_PATTERN.test(configuredName)
  ) {
    return configuredName;
  }

  return generatedLabel;
};

const chartTimeAxisFormatter = new Intl.DateTimeFormat(undefined, {
  hour: "2-digit",
  minute: "2-digit",
});

const chartTimeCrosshairFormatter = new Intl.DateTimeFormat(undefined, {
  hour: "2-digit",
  minute: "2-digit",
  second: "2-digit",
});

function chartTimeToDate(time: Time): Date | null {
  if (typeof time === "number") {
    return new Date(time * 1000);
  }

  if (typeof time === "string") {
    const [year, month, day] = time.split("-").map(Number);

    if (!year || !month || !day) {
      return null;
    }

    return new Date(year, month - 1, day);
  }

  return new Date(time.year, time.month - 1, time.day);
}

function formatChartLocalTime(
  time: Time,
  formatter: Intl.DateTimeFormat
): string {
  const date = chartTimeToDate(time);

  return date ? formatter.format(date) : "";
}

export function StrategyPage() {
  const { getAccessTokenSilently } = useAuth0();

  const [isStrategyConfigOpen, setIsStrategyConfigOpen] = React.useState(false);
  const [showParametersOnly, setShowParametersOnly] = React.useState(false);
  const [showDisconnectButton, setShowDisconnectButton] = React.useState(false);
  const [selectedStrategyId, setSelectedStrategyId] = React.useState("");
  const [strategyFormVersion, setStrategyFormVersion] = React.useState(0);
  const [showStartButton, setShowStartButton] = React.useState(false);
  const [showChart, setShowChart] = React.useState(false);
  const [indicatorSeriesKeys, setIndicatorSeriesKeys] = React.useState<
    string[]
  >([]);
  const [chartSeriesVisibility, setChartSeriesVisibility] =
    React.useState<ChartSeriesVisibility>({
      [PRICE_SERIES_KEY]: true,
      [VOLUME_SERIES_KEY]: true,
    });
  const [chartSeriesLabelPositions, setChartSeriesLabelPositions] =
    React.useState<ChartSeriesLabelPosition[]>([]);

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
  const [symbolExchange, setSymbolExchange] = React.useState<
    ApiSymbol["exchange"] | null
  >(null);

  const latestStrategyRef = React.useRef<Strategy | null>(null);
  const previousPriceRef = React.useRef<number | null>(null);
  const chartRef = React.useRef<HTMLDivElement | null>(null);
  const chartApiRef = React.useRef<IChartApi | null>(null);
  const candleSeriesRef = React.useRef<ISeriesApi<"Candlestick"> | null>(null);
  const volumeSeriesRef = React.useRef<ISeriesApi<"Histogram"> | null>(null);
  const signalMarkersRef = React.useRef<ISeriesMarkersPluginApi<Time> | null>(
    null
  );
  const signalMarkersDataRef = React.useRef<SeriesMarker<Time>[]>([]);
  const indicatorSeriesByKeyRef = React.useRef<Map<string, ISeriesApi<"Line">>>(
    new Map()
  );
  const indicatorSeriesDataRef = React.useRef<IndicatorSeriesData[]>([]);
  const chartSeriesVisibilityRef = React.useRef<ChartSeriesVisibility>({
    [PRICE_SERIES_KEY]: true,
    [VOLUME_SERIES_KEY]: true,
  });
  const candleDataByTimeRef = React.useRef<
    Map<number, CandlestickData<UTCTimestamp>>
  >(new Map());
  const volumeDataByTimeRef = React.useRef<
    Map<number, HistogramData<UTCTimestamp>>
  >(new Map());
  const hasAppliedInitialVisibleKlineRangeRef = React.useRef(false);
  const hasUserInteractedWithChartRef = React.useRef(false);

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
  const exchangeLabel =
    symbolExchange === null || symbolExchange === Exchange.None
      ? null
      : ExchangeLabels[symbolExchange];

  const applySymbol = (nextSymbol: ApiSymbol | null) => {
    setSymbol(nextSymbol);
    setSymbolName(nextSymbol?.name ?? nextSymbol?.exchangeSymbol ?? null);
    setSymbolExchange(nextSymbol?.exchange ?? null);
  };

  const getIndicatorSeriesLabel = React.useCallback(
    (key: string) => {
      const periods = strategy?.periods;

      if (!periods) {
        return key;
      }

      const period =
        periods[key] ??
        Object.values(periods).find((period) => {
          const configuredName = period.name?.trim();

          return (
            configuredName === key ||
            formatMovingAveragePeriodLabel(period) === key
          );
        });

      return period ? formatMovingAveragePeriodLabel(period) : key;
    },
    [strategy?.periods]
  );

  const resetPriceComparison = () => {
    previousPriceRef.current = null;
    setPrice(null);
    setPriceDirection("flat");
  };

  const getApiUrl = (baseUrl: string, route: string) => {
    const normalizedBaseUrl = baseUrl.endsWith("/") ? baseUrl : `${baseUrl}/`;
    return new URL(route, normalizedBaseUrl).toString();
  };

  const getChartSeriesKeys = (
    indicatorSeriesData = indicatorSeriesDataRef.current
  ) => [
    PRICE_SERIES_KEY,
    VOLUME_SERIES_KEY,
    ...indicatorSeriesData.map((indicatorSeries) => indicatorSeries.key),
  ];

  const syncChartSeriesVisibility = (
    indicatorSeriesData = indicatorSeriesDataRef.current
  ) => {
    const keys = getChartSeriesKeys(indicatorSeriesData);
    const currentVisibility = chartSeriesVisibilityRef.current;
    const nextVisibility = Object.fromEntries(
      keys.map((key) => [key, currentVisibility[key] ?? true])
    );
    const hasChanged =
      Object.keys(currentVisibility).length !== keys.length ||
      keys.some((key) => currentVisibility[key] !== nextVisibility[key]);

    if (hasChanged) {
      chartSeriesVisibilityRef.current = nextVisibility;
      setChartSeriesVisibility(nextVisibility);
    }
  };

  const isChartSeriesVisible = (key: string) =>
    chartSeriesVisibilityRef.current[key] ?? true;

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

  const toVolumeData = (kline: Kline): HistogramData<UTCTimestamp> => ({
    time: toChartTime(kline.openTime),
    value: kline.volume,
    color:
      kline.close < kline.open
        ? VOLUME_SERIES_DOWN_COLOR
        : VOLUME_SERIES_UP_COLOR,
  });

  const sortByTime = React.useCallback(function sortByTime<
    T extends { time: UTCTimestamp },
  >(items: T[]) {
    return items.sort((a, b) => a.time - b.time);
  }, []);

  const getPriceSeriesColor = (candle: CandlestickData<UTCTimestamp>) =>
    candle.close < candle.open
      ? PRICE_SERIES_DOWN_COLOR
      : PRICE_SERIES_UP_COLOR;

  const getLatestCandle = React.useCallback(
    () => sortByTime([...candleDataByTimeRef.current.values()]).at(-1) ?? null,
    [sortByTime]
  );

  const syncPriceSeriesColor = React.useCallback(() => {
    const latestCandle = getLatestCandle();

    if (!latestCandle) {
      return;
    }

    candleSeriesRef.current?.applyOptions({
      priceLineColor: getPriceSeriesColor(latestCandle),
    });
  }, [getLatestCandle]);

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

  const clearIndicatorSeries = React.useCallback(() => {
    const chart = chartApiRef.current;

    if (chart) {
      for (const indicatorSeries of indicatorSeriesByKeyRef.current.values()) {
        chart.removeSeries(indicatorSeries);
      }
    }

    indicatorSeriesByKeyRef.current = new Map();
  }, []);

  const updateChartSeriesLabelPositions = React.useCallback(() => {
    const chart = chartApiRef.current;
    const container = chartRef.current;

    if (!chart || !container) {
      setChartSeriesLabelPositions([]);
      return;
    }

    const timeScale = chart.timeScale();
    const visibleRange = timeScale.getVisibleRange();
    const from = visibleRange ? Number(visibleRange.from) : null;
    const to = visibleRange ? Number(visibleRange.to) : null;
    const maxX = Math.max(8, container.clientWidth - 160);
    const maxY = Math.max(6, container.clientHeight - 22);

    const nextLabelPositions = indicatorSeriesDataRef.current.flatMap(
      ({ key, data }, index) => {
        if (!isChartSeriesVisible(key)) {
          return [];
        }

        const indicatorSeries = indicatorSeriesByKeyRef.current.get(key);

        if (!indicatorSeries) {
          return [];
        }

        const labelPoint =
          data.find((point) => {
            const time = Number(point.time);

            return (
              (from === null || time >= from) && (to === null || time <= to)
            );
          }) ?? data[0];

        if (!labelPoint) {
          return [];
        }

        const x = timeScale.timeToCoordinate(labelPoint.time);
        const y = indicatorSeries.priceToCoordinate(labelPoint.value);

        if (x === null || y === null) {
          return [];
        }

        return [
          {
            key,
            label: getIndicatorSeriesLabel(key),
            color: getIndicatorSeriesColor(index),
            x: Math.min(maxX, Math.max(8, x + 8)),
            y: Math.min(maxY, Math.max(6, y - 10)),
          },
        ];
      }
    );

    setChartSeriesLabelPositions(nextLabelPositions);
  }, [getIndicatorSeriesLabel]);

  const preserveVisibleLogicalRange = React.useCallback(
    (applyChartChanges: () => void, afterChartChanges?: () => void) => {
      const chart = chartApiRef.current;
      const timeScale = chart?.timeScale();
      const shouldPreserveVisibleRange =
        hasUserInteractedWithChartRef.current &&
        hasAppliedInitialVisibleKlineRangeRef.current &&
        timeScale !== undefined &&
        Math.abs(timeScale.scrollPosition()) > MANUAL_SCROLL_THRESHOLD;
      const visibleLogicalRange = shouldPreserveVisibleRange
        ? timeScale.getVisibleLogicalRange()
        : null;

      applyChartChanges();

      if (timeScale && visibleLogicalRange) {
        timeScale.setVisibleLogicalRange(visibleLogicalRange);
        afterChartChanges?.();

        window.requestAnimationFrame(() => {
          timeScale.setVisibleLogicalRange(visibleLogicalRange);
          afterChartChanges?.();
        });
        return;
      }

      afterChartChanges?.();
    },
    []
  );

  const applySignalMarkersToChart = React.useCallback(() => {
    const candleSeries = candleSeriesRef.current;

    if (!candleSeries) {
      return;
    }

    preserveVisibleLogicalRange(() => {
      const signalMarkers =
        signalMarkersRef.current ?? createSeriesMarkers(candleSeries);

      signalMarkersRef.current = signalMarkers;
      signalMarkers.setMarkers(signalMarkersDataRef.current);
    }, updateChartSeriesLabelPositions);
  }, [preserveVisibleLogicalRange, updateChartSeriesLabelPositions]);

  const addIndicatorSeriesToChart = React.useCallback(() => {
    const chart = chartApiRef.current;

    if (!chart) {
      return;
    }

    preserveVisibleLogicalRange(() => {
      clearIndicatorSeries();

      const indicatorSeriesByKey = new Map<string, ISeriesApi<"Line">>();

      for (const [
        index,
        { key, data },
      ] of indicatorSeriesDataRef.current.entries()) {
        if (!isChartSeriesVisible(key)) {
          continue;
        }

        const color = getIndicatorSeriesColor(index);
        const indicatorSeries = chart.addSeries(LineSeries, {
          color,
          lineWidth: 2,
          priceLineColor: color,
          priceLineVisible: true,
          lastValueVisible: true,
          priceScaleId: "right",
        });

        indicatorSeries.setData(data);
        indicatorSeriesByKey.set(key, indicatorSeries);
      }

      indicatorSeriesByKeyRef.current = indicatorSeriesByKey;
    }, updateChartSeriesLabelPositions);
  }, [
    clearIndicatorSeries,
    preserveVisibleLogicalRange,
    updateChartSeriesLabelPositions,
  ]);

  const applyKlinesToChart = (klines: Kline[], replace = false) => {
    const candleSeries = candleSeriesRef.current;
    const volumeSeries = volumeSeriesRef.current;
    const nextCandles = klines.map(toCandleData);
    const nextVolumes = klines.map(toVolumeData);

    if (replace) {
      candleDataByTimeRef.current = new Map();
      volumeDataByTimeRef.current = new Map();
    }

    for (const candle of nextCandles) {
      candleDataByTimeRef.current.set(candle.time, candle);
    }

    for (const volume of nextVolumes) {
      volumeDataByTimeRef.current.set(volume.time, volume);
    }

    if (!replace && candleSeries && volumeSeries) {
      preserveVisibleLogicalRange(() => {
        for (const candle of nextCandles) {
          candleSeries.update(candle);
        }
        for (const volume of nextVolumes) {
          volumeSeries.update(volume);
        }
        syncPriceSeriesColor();
      }, updateChartSeriesLabelPositions);
    }

    if (!replace && (!candleSeries || !volumeSeries)) {
      return;
    }

    if (replace && candleSeries && volumeSeries) {
      preserveVisibleLogicalRange(() => {
        candleSeries.setData(
          sortByTime([...candleDataByTimeRef.current.values()])
        );
        volumeSeries.setData(
          sortByTime([...volumeDataByTimeRef.current.values()])
        );
        syncPriceSeriesColor();
      }, updateChartSeriesLabelPositions);
      applyInitialVisibleKlineRange();
    }
  };

  const applyInitialVisibleKlineRange = () => {
    const chart = chartApiRef.current;
    const container = chartRef.current;
    const klineCount = candleDataByTimeRef.current.size;

    if (
      !chart ||
      !container ||
      klineCount === 0 ||
      hasAppliedInitialVisibleKlineRangeRef.current
    ) {
      return;
    }

    const visibleKlineCount = Math.min(klineCount, INITIAL_VISIBLE_KLINE_LIMIT);
    const initialBarSpacing = Math.max(
      MIN_INITIAL_BAR_SPACING,
      Math.min(
        MAX_INITIAL_BAR_SPACING,
        container.clientWidth / visibleKlineCount
      )
    );

    chart.timeScale().applyOptions({
      barSpacing: initialBarSpacing,
    });
    chart.timeScale().scrollToPosition(0, false);
    hasAppliedInitialVisibleKlineRangeRef.current = true;
  };

  const applyIndicatorsToChart = (indicators: Indicators[]) => {
    const indicatorSeriesData = toIndicatorSeriesData(indicators);

    indicatorSeriesDataRef.current = indicatorSeriesData;
    setIndicatorSeriesKeys(
      indicatorSeriesData.map((indicatorSeries) => indicatorSeries.key)
    );
    syncChartSeriesVisibility(indicatorSeriesData);
    addIndicatorSeriesToChart();
    applyInitialVisibleKlineRange();
    updateChartSeriesLabelPositions();
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

    const indicatorSeriesData = [...seriesByKey.entries()].map(
      ([key, data]) => ({
        key,
        data,
      })
    );
    indicatorSeriesDataRef.current = indicatorSeriesData;
    setIndicatorSeriesKeys(
      indicatorSeriesData.map((indicatorSeries) => indicatorSeries.key)
    );
    syncChartSeriesVisibility(indicatorSeriesData);
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
    volumeDataByTimeRef.current = new Map();
    indicatorSeriesDataRef.current = [];
    signalMarkersDataRef.current = [];
    hasAppliedInitialVisibleKlineRangeRef.current = false;
    hasUserInteractedWithChartRef.current = false;
    chartSeriesVisibilityRef.current = {
      [PRICE_SERIES_KEY]: true,
      [VOLUME_SERIES_KEY]: true,
    };
    setShowChart(false);
    setIndicatorSeriesKeys([]);
    setChartSeriesVisibility({
      [PRICE_SERIES_KEY]: true,
      [VOLUME_SERIES_KEY]: true,
    });
    setChartSeriesLabelPositions([]);
    candleSeriesRef.current?.setData([]);
    volumeSeriesRef.current?.setData([]);
    signalMarkersRef.current?.setMarkers([]);
    clearIndicatorSeries();
  };

  const handleChartSeriesVisibilityChange = (
    key: string,
    checked: boolean | "indeterminate"
  ) => {
    const nextVisibility = {
      ...chartSeriesVisibilityRef.current,
      [key]: checked === true,
    };

    chartSeriesVisibilityRef.current = nextVisibility;
    setChartSeriesVisibility(nextVisibility);

    if (key === PRICE_SERIES_KEY) {
      candleSeriesRef.current?.applyOptions({
        visible: nextVisibility[PRICE_SERIES_KEY],
      });
      return;
    }

    if (key === VOLUME_SERIES_KEY) {
      volumeSeriesRef.current?.applyOptions({
        visible: nextVisibility[VOLUME_SERIES_KEY],
      });
      return;
    }

    addIndicatorSeriesToChart();
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
          applyRunningStrategyStatus(payload.strategy, null);
          applySymbol(payload.symbol);
          applyKlinesToChart(payload.klines, true);
          if (payload.klines.length > 0) {
            setShowChart(true);
          }
          applyIndicatorsToChart(payload.indicators);
          applySignalsToChart(payload.signals);

          setNotificationMessage(null);
        }

        break;
      }
      case MessageType.Kline: {
        const payload = envelope.payload as Kline | undefined;

        if (payload) {
          applyKlinesToChart([payload]);
          setShowChart(true);
        }

        setNotificationMessage(null);
        break;
      }
      case MessageType.Indicator: {
        const payload = envelope.payload as Indicators | undefined;

        if (payload) {
          applyIndicatorToChart(payload);
        }

        setNotificationMessage(null);
        break;
      }
      case MessageType.Signal: {
        const payload = envelope.payload as Signal | undefined;

        if (payload) {
          applySignalToChart(payload);
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
    resetChartData();
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

    if (!showStrategyRunning || !showChart || !container) {
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
        vertLines: { color: "#e2e8f0", style: 2 },
        horzLines: { color: "#e2e8f0", style: 2 },
      },
      crosshair: {
        mode: CrosshairMode.Normal,
      },
      localization: {
        timeFormatter: (time: Time) =>
          formatChartLocalTime(time, chartTimeCrosshairFormatter),
      },
      rightPriceScale: {
        borderColor: "#cbd5e1",
        scaleMargins: {
          top: 0.05,
          bottom: 0.26,
        },
      },
      leftPriceScale: {
        borderColor: "#cbd5e1",
        visible: false,
      },
      timeScale: {
        borderColor: "#cbd5e1",
        timeVisible: true,
        secondsVisible: true,
        tickMarkFormatter: (time: Time) =>
          formatChartLocalTime(time, chartTimeAxisFormatter),
      },
    });

    const candleSeries = chart.addSeries(CandlestickSeries, {
      upColor: PRICE_SERIES_UP_COLOR,
      downColor: PRICE_SERIES_DOWN_COLOR,
      borderUpColor: PRICE_SERIES_UP_COLOR,
      borderDownColor: PRICE_SERIES_DOWN_COLOR,
      wickUpColor: PRICE_SERIES_UP_COLOR,
      wickDownColor: PRICE_SERIES_DOWN_COLOR,
      priceLineColor: PRICE_SERIES_COLOR,
      lastValueVisible: true,
      visible: isChartSeriesVisible(PRICE_SERIES_KEY),
    });

    const volumeSeries = chart.addSeries(HistogramSeries, {
      color: VOLUME_SERIES_COLOR,
      priceFormat: {
        type: "volume",
      },
      priceLineVisible: false,
      lastValueVisible: false,
      priceScaleId: VOLUME_SERIES_KEY,
      visible: isChartSeriesVisible(VOLUME_SERIES_KEY),
    });

    chart.priceScale(VOLUME_SERIES_KEY).applyOptions({
      scaleMargins: {
        top: 0.78,
        bottom: 0,
      },
      borderVisible: false,
    });

    chartApiRef.current = chart;
    candleSeriesRef.current = candleSeries;
    volumeSeriesRef.current = volumeSeries;

    candleSeries.setData(sortByTime([...candleDataByTimeRef.current.values()]));
    volumeSeries.setData(sortByTime([...volumeDataByTimeRef.current.values()]));
    syncPriceSeriesColor();
    applySignalMarkersToChart();
    addIndicatorSeriesToChart();
    applyInitialVisibleKlineRange();
    updateChartSeriesLabelPositions();

    const handleChartInteraction = () => {
      hasUserInteractedWithChartRef.current = true;
    };

    const handleVisibleTimeRangeChange = () => {
      if (
        Math.abs(chart.timeScale().scrollPosition()) <= MANUAL_SCROLL_THRESHOLD
      ) {
        hasUserInteractedWithChartRef.current = false;
      }

      updateChartSeriesLabelPositions();
    };

    container.addEventListener("pointerdown", handleChartInteraction);
    container.addEventListener("wheel", handleChartInteraction, {
      passive: true,
    });
    container.addEventListener("touchstart", handleChartInteraction, {
      passive: true,
    });

    chart
      .timeScale()
      .subscribeVisibleTimeRangeChange(handleVisibleTimeRangeChange);

    return () => {
      container.removeEventListener("pointerdown", handleChartInteraction);
      container.removeEventListener("wheel", handleChartInteraction);
      container.removeEventListener("touchstart", handleChartInteraction);
      chart
        .timeScale()
        .unsubscribeVisibleTimeRangeChange(handleVisibleTimeRangeChange);
      signalMarkersRef.current?.detach();
      chartApiRef.current = null;
      candleSeriesRef.current = null;
      volumeSeriesRef.current = null;
      signalMarkersRef.current = null;
      indicatorSeriesByKeyRef.current = new Map();
      setChartSeriesLabelPositions([]);
      chart.remove();
    };
  }, [
    addIndicatorSeriesToChart,
    applySignalMarkersToChart,
    showChart,
    showStrategyRunning,
    sortByTime,
    syncPriceSeriesColor,
    symbolName,
    updateChartSeriesLabelPositions,
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
    []
  );

  const chartSeriesControls = [
    {
      key: PRICE_SERIES_KEY,
      label: hasSymbol ? (symbolName ?? "Price") : "Price",
      color: PRICE_SERIES_COLOR,
    },
    {
      key: VOLUME_SERIES_KEY,
      label: "Volume",
      color: VOLUME_SERIES_COLOR,
    },
    ...indicatorSeriesKeys.map((key, index) => ({
      key,
      label: getIndicatorSeriesLabel(key),
      color: getIndicatorSeriesColor(index),
    })),
  ];
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

        <StrategyHeader
          connectError={connectError}
          notificationMessage={notificationMessage}
          showStrategyRunning={showStrategyRunning}
          strategy={strategy}
          isStrategyParametersActive={isStrategyParametersActive}
          isStrategyConfigActive={isStrategyConfigActive}
          onToggleStrategyParameters={handleToggleStrategyParameters}
          onToggleStrategyConfig={handleToggleStrategyConfig}
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
                <CardTitle className="flex min-h-5 items-baseline gap-1">
                  {hasSymbol ? (
                    <>
                      {exchangeLabel ? (
                        <h4 className="text-sm text-foreground-semimuted mr-2">
                          {exchangeLabel}
                        </h4>
                      ) : null}
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
                </CardTitle>
              </CardHeader>
              <CardContent className="flex min-h-0 flex-1 flex-col">
                {showChart ? (
                  <>
                    <div
                      className="mb-2 flex min-h-7 flex-wrap items-center gap-x-4 gap-y-2"
                      aria-label="Chart series visibility"
                    >
                      {chartSeriesControls.map((series) => (
                        <label
                          key={series.key}
                          className="flex min-w-0 cursor-pointer items-center gap-2 text-xs font-medium text-muted-foreground"
                        >
                          <Checkbox
                            checked={chartSeriesVisibility[series.key] ?? true}
                            onCheckedChange={(checked) =>
                              handleChartSeriesVisibilityChange(
                                series.key,
                                checked
                              )
                            }
                            aria-label={`Show ${series.label} series`}
                          />
                          <span
                            className="size-2.5 shrink-0 rounded-full"
                            style={{ backgroundColor: series.color }}
                            aria-hidden="true"
                          />
                          <span className="truncate">{series.label}</span>
                        </label>
                      ))}
                    </div>
                    <div className="relative min-h-0 flex-1 w-full">
                      {chartSeriesLabelPositions.map((series) => (
                        <div
                          key={series.key}
                          className="pointer-events-none absolute z-10 flex max-w-36 items-center gap-1.5 rounded bg-background/85 px-1.5 py-0.5 text-[10px] font-medium leading-none text-foreground shadow-sm"
                          style={{
                            left: series.x,
                            top: series.y,
                          }}
                        >
                          <span
                            className="size-2 shrink-0 rounded-full"
                            style={{ backgroundColor: series.color }}
                            aria-hidden="true"
                          />
                          <span className="truncate">{series.label}</span>
                        </div>
                      ))}
                      <div ref={chartRef} className="h-full w-full" />
                    </div>
                    <a
                      href="https://www.tradingview.com/"
                      target="_blank"
                      rel="noreferrer"
                      className="mt-1 text-left text-[10px] leading-none text-muted-foreground hover:text-foreground"
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
