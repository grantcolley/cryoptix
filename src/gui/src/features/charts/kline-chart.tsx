import * as React from "react";
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
import type { Strategy } from "@/features/api/schema/strategy-schema";
import type { Kline } from "@/features/api/schema/kline-schema";
import type { Indicators } from "@/features/api/schema/indicators-schema";
import type { Signal } from "@/features/api/schema/signal-schema";
import { SignalType } from "@/features/api/schema/signal-type";
import { IndicatorTypeLabels } from "@/features/api/schema/indicator-type";
import { Checkbox } from "@/components/ui/checkbox";
import {
  attachKlineChart,
  type KlineChartController,
} from "./kline-chart-controller";
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
const LIGHT_GRID_COLOR = "#e2e8f0";
const DARK_GRID_COLOR = "#273449";
const INITIAL_VISIBLE_KLINE_LIMIT = 120;
const MIN_INITIAL_BAR_SPACING = 3;
const MAX_INITIAL_BAR_SPACING = 12;
const MANUAL_SCROLL_THRESHOLD = 0.5;
const INDICATOR_LABEL_PATTERN = /^\d+\s+[a-z]+$/i;

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

const formatIndicatorLabel = (indicator: Strategy["indicators"][number]) => {
  const generatedLabel = [
    ...indicator.values.map(({ value }) => value),
    IndicatorTypeLabels[indicator.indicatorType],
  ].join(" ");
  const configuredName = indicator.name?.trim();

  if (
    configuredName &&
    configuredName !== generatedLabel &&
    !INDICATOR_LABEL_PATTERN.test(configuredName)
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

function getIndicatorSeriesLabel(
  key: string,
  indicators?: Strategy["indicators"]
) {
  if (!indicators) return key;
  const indicator =
    indicators.find((indicator) => indicator.id === key) ??
    indicators.find((indicator) => {
      const configuredName = indicator.name?.trim();
      return configuredName === key || formatIndicatorLabel(indicator) === key;
    });
  return indicator ? formatIndicatorLabel(indicator) : key;
}

interface KlineChartProps {
  controller: KlineChartController;
  indicators?: Strategy["indicators"];
  priceLabel?: string;
  onRightScaleWidthChange?: (width: number) => void;
}

export function KlineChart({
  controller,
  indicators,
  priceLabel = "Price",
  onRightScaleWidthChange,
}: KlineChartProps) {
  const indicatorsRef = React.useRef(indicators);
  const onRightScaleWidthChangeRef = React.useRef(onRightScaleWidthChange);
  React.useLayoutEffect(() => {
    indicatorsRef.current = indicators;
    onRightScaleWidthChangeRef.current = onRightScaleWidthChange;
  }, [indicators, onRightScaleWidthChange]);
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
  const [hoveredKline, setHoveredKline] = React.useState<Kline | null>(null);
  const [hoveredKlinePosition, setHoveredKlinePosition] = React.useState({
    x: 8,
    y: 8,
  });
  const hoveredKlineTooltipRef = React.useRef<HTMLDListElement | null>(null);
  const hoveredKlineTimeRef = React.useRef<number | null>(null);

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

  const pendingFramesRef = React.useRef<Set<number>>(new Set());
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

      for (const item of indicator.series) {
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

    onRightScaleWidthChangeRef.current?.(
      chart?.priceScale("right").width() ?? 0
    );

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
            label: getIndicatorSeriesLabel(key, indicatorsRef.current),
            color: getIndicatorSeriesColor(index),
            x: Math.min(maxX, Math.max(8, x + 8)),
            y: Math.min(maxY, Math.max(6, y - 10)),
          },
        ];
      }
    );

    setChartSeriesLabelPositions(nextLabelPositions);
  }, []);

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

        const frame = window.requestAnimationFrame(() => {
          pendingFramesRef.current.delete(frame);
          if (chartApiRef.current !== chart) return;
          timeScale.setVisibleLogicalRange(visibleLogicalRange);
          afterChartChanges?.();
        });
        pendingFramesRef.current.add(frame);
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

    if (hoveredKlineTimeRef.current !== null) {
      setHoveredKline(controller.getKline(hoveredKlineTimeRef.current) ?? null);
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

    for (const item of indicator.series) {
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
    for (const frame of pendingFramesRef.current) cancelAnimationFrame(frame);
    pendingFramesRef.current.clear();
    candleDataByTimeRef.current = new Map();
    volumeDataByTimeRef.current = new Map();
    indicatorSeriesDataRef.current = [];
    signalMarkersDataRef.current = [];
    hoveredKlineTimeRef.current = null;
    setHoveredKline(null);
    hasAppliedInitialVisibleKlineRangeRef.current = false;
    hasUserInteractedWithChartRef.current = false;
    chartSeriesVisibilityRef.current = {
      [PRICE_SERIES_KEY]: true,
      [VOLUME_SERIES_KEY]: true,
    };
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

  React.useEffect(() => {
    const container = chartRef.current;
    const pendingFrames = pendingFramesRef.current;

    if (!container) {
      return;
    }

    hasAppliedInitialVisibleKlineRangeRef.current = false;
    hasUserInteractedWithChartRef.current = false;

    const gridColor = document.documentElement.classList.contains("dark")
      ? DARK_GRID_COLOR
      : LIGHT_GRID_COLOR;
    const chart = createChart(container, {
      autoSize: true,
      layout: {
        attributionLogo: false,
        background: { type: ColorType.Solid, color: "transparent" },
        textColor: "#475569",
      },
      grid: {
        vertLines: { color: gridColor, style: 2 },
        horzLines: { color: gridColor, style: 2 },
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

    const detachController = attachKlineChart(controller, {
      replaceSnapshot: (snapshot) => {
        applyKlinesToChart(snapshot.klines, true);
        applyIndicatorsToChart(snapshot.indicators);
        applySignalsToChart(snapshot.signals);
      },
      applyKline: (kline) => applyKlinesToChart([kline]),
      applyIndicator: applyIndicatorToChart,
      applySignal: applySignalToChart,
      reset: resetChartData,
    });

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
    chart.timeScale().subscribeSizeChange(updateChartSeriesLabelPositions);

    const themeObserver = new MutationObserver(() => {
      const nextGridColor = document.documentElement.classList.contains("dark")
        ? DARK_GRID_COLOR
        : LIGHT_GRID_COLOR;
      chart.applyOptions({
        grid: {
          vertLines: { color: nextGridColor, style: 2 },
          horzLines: { color: nextGridColor, style: 2 },
        },
      });
    });
    themeObserver.observe(document.documentElement, {
      attributes: true,
      attributeFilter: ["class"],
    });

    const handleCrosshairMove = (
      param: import("lightweight-charts").MouseEventParams<Time>
    ) => {
      if (param.time === undefined || !param.point) {
        hoveredKlineTimeRef.current = null;
        setHoveredKline(null);
        return;
      }

      const container = chartRef.current;
      if (container) {
        const tooltip = hoveredKlineTooltipRef.current;
        const tooltipWidth = tooltip?.offsetWidth ?? 190;
        const tooltipHeight = tooltip?.offsetHeight ?? 32;
        const gap = 12;
        const x = Math.min(
          Math.max(4, param.point.x + gap),
          Math.max(4, container.clientWidth - tooltipWidth - 4)
        );
        const belowY = param.point.y + gap;
        const y = Math.min(
          Math.max(
            4,
            belowY + tooltipHeight <= container.clientHeight
              ? belowY
              : param.point.y - tooltipHeight - gap
          ),
          Math.max(4, container.clientHeight - tooltipHeight - 4)
        );
        setHoveredKlinePosition({ x, y });
      }

      const time = Number(param.time);
      hoveredKlineTimeRef.current = time;
      setHoveredKline(controller.getKline(time) ?? null);
    };
    chart.subscribeCrosshairMove(handleCrosshairMove);

    return () => {
      detachController();
      for (const frame of pendingFrames) cancelAnimationFrame(frame);
      pendingFrames.clear();
      container.removeEventListener("pointerdown", handleChartInteraction);
      container.removeEventListener("wheel", handleChartInteraction);
      container.removeEventListener("touchstart", handleChartInteraction);
      chart
        .timeScale()
        .unsubscribeVisibleTimeRangeChange(handleVisibleTimeRangeChange);
      chart.timeScale().unsubscribeSizeChange(updateChartSeriesLabelPositions);
      themeObserver.disconnect();
      chart.unsubscribeCrosshairMove(handleCrosshairMove);
      hoveredKlineTimeRef.current = null;
      setHoveredKline(null);
      signalMarkersRef.current?.detach();
      chartApiRef.current = null;
      candleSeriesRef.current = null;
      volumeSeriesRef.current = null;
      signalMarkersRef.current = null;
      indicatorSeriesByKeyRef.current = new Map();
      setChartSeriesLabelPositions([]);
      onRightScaleWidthChangeRef.current?.(0);
      chart.remove();
    };
    // Chart lifetime follows the controller, not presentation metadata.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [controller]);
  React.useEffect(() => {
    updateChartSeriesLabelPositions();
  }, [indicators, updateChartSeriesLabelPositions]);
  const chartSeriesControls = [
    {
      key: PRICE_SERIES_KEY,
      label: priceLabel,
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
                handleChartSeriesVisibilityChange(series.key, checked)
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
        {hoveredKline ? (
          <dl
            ref={hoveredKlineTooltipRef}
            className="pointer-events-none absolute z-20 flex flex-wrap gap-x-3 gap-y-1 rounded-md border bg-background/90 px-2 py-1 text-xs tabular-nums shadow-sm"
            aria-label="Hovered candle values"
            style={{
              left: hoveredKlinePosition.x,
              top: hoveredKlinePosition.y,
            }}
          >
            <div className="flex gap-1">
              <dt className="text-muted-foreground">O</dt>
              <dd>{hoveredKline.open}</dd>
            </div>
            <div className="flex gap-1">
              <dt className="text-muted-foreground">H</dt>
              <dd>{hoveredKline.high}</dd>
            </div>
            <div className="flex gap-1">
              <dt className="text-muted-foreground">L</dt>
              <dd>{hoveredKline.low}</dd>
            </div>
            <div className="flex gap-1">
              <dt className="text-muted-foreground">C</dt>
              <dd>{hoveredKline.close}</dd>
            </div>
          </dl>
        ) : null}
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
  );
}
