import * as React from "react";
import {
  ColorType,
  CrosshairMode,
  HistogramSeries,
  LineSeries,
  createChart,
  type HistogramData,
  type IChartApi,
  type ISeriesApi,
  type LineData,
  type Time,
  type UTCTimestamp,
} from "lightweight-charts";
import type { Indicators } from "@/features/api/schema/indicators-schema";
import { Checkbox } from "@/components/ui/checkbox";

type MacdPoint = {
  name: string;
  time: UTCTimestamp;
  value: number;
  signal: number;
  histogram: number;
};

const LIGHT_GRID_COLOR = "#e2e8f0";
const DARK_GRID_COLOR = "#273449";
const INITIAL_VISIBLE_POINT_LIMIT = 120;
const MIN_INITIAL_BAR_SPACING = 3;
const MAX_INITIAL_BAR_SPACING = 12;
const MANUAL_SCROLL_THRESHOLD = 0.5;
const MACD_SERIES = [
  { key: "macd", label: "MACD", color: "#2563eb" },
  { key: "signal", label: "Signal", color: "#f97316" },
  { key: "histogram", label: "Histogram", color: "#64748b" },
] as const;
type MacdSeriesKey = (typeof MACD_SERIES)[number]["key"];
type MacdSeriesLabelPosition = {
  key: MacdSeriesKey;
  label: string;
  color: string;
  x: number;
  y: number;
};

function toMacdPoints(indicators: Indicators[]): MacdPoint[] {
  const points = new Map<string, MacdPoint>();

  for (const indicator of indicators) {
    const time = Math.floor(indicator.timestampUtc.getTime() / 1000);
    for (const [key, macd] of Object.entries(indicator.macds)) {
      points.set(String(time), {
        name: macd.name ?? key,
        time: time as UTCTimestamp,
        value: macd.value,
        signal: macd.signal,
        histogram: macd.histogram,
      });
    }
  }

  return [...points.values()].sort((a, b) => a.time - b.time);
}

export function MacdChart({ indicators }: { indicators: Indicators[] }) {
  const chartRef = React.useRef<HTMLDivElement>(null);
  const tooltipRef = React.useRef<HTMLDListElement>(null);
  const [seriesLabelPositions, setSeriesLabelPositions] = React.useState<
    MacdSeriesLabelPosition[]
  >([]);
  const pointsByTimeRef = React.useRef(new Map<number, MacdPoint>());
  const [hoveredPoint, setHoveredPoint] = React.useState<MacdPoint | null>(
    null
  );
  const [tooltipPosition, setTooltipPosition] = React.useState({ x: 8, y: 8 });
  const seriesRef = React.useRef<{
    chart: IChartApi;
    macd: ISeriesApi<"Line">;
    signal: ISeriesApi<"Line">;
    histogram: ISeriesApi<"Histogram">;
  } | null>(null);
  const points = React.useMemo(() => toMacdPoints(indicators), [indicators]);
  const macdDataRef = React.useRef<LineData<UTCTimestamp>[]>([]);
  const signalDataRef = React.useRef<LineData<UTCTimestamp>[]>([]);
  const histogramDataRef = React.useRef<HistogramData<UTCTimestamp>[]>([]);
  const [visibility, setVisibility] = React.useState<Record<string, boolean>>({
    macd: true,
    signal: true,
    histogram: true,
  });
  const visibilityRef = React.useRef(visibility);
  const updateSeriesLabelPositionsRef = React.useRef<(() => void) | null>(null);
  const hasUserInteractedWithChartRef = React.useRef(false);

  const handleVisibilityChange = (
    key: string,
    checked: boolean | "indeterminate"
  ) => {
    const visible = checked === true;
    visibilityRef.current = { ...visibilityRef.current, [key]: visible };
    setVisibility((current) => ({ ...current, [key]: visible }));
    const series = seriesRef.current;
    if (!series) return;
    if (key === "macd" || key === "signal" || key === "histogram") {
      series[key].applyOptions({ visible });
      updateSeriesLabelPositionsRef.current?.();
    }
  };

  React.useEffect(() => {
    const container = chartRef.current;
    if (!container) return;

    const isDarkMode = document.documentElement.classList.contains("dark");
    const gridColor = isDarkMode ? DARK_GRID_COLOR : LIGHT_GRID_COLOR;
    const chart = createChart(container, {
      autoSize: true,
      layout: {
        background: { type: ColorType.Solid, color: "transparent" },
        textColor: "#64748b",
        attributionLogo: false,
      },
      grid: {
        vertLines: { color: gridColor, style: 2 },
        horzLines: { color: gridColor, style: 2 },
      },
      crosshair: { mode: CrosshairMode.Normal },
      rightPriceScale: { borderVisible: isDarkMode, borderColor: gridColor },
      timeScale: {
        borderVisible: isDarkMode,
        borderColor: gridColor,
        timeVisible: true,
        secondsVisible: false,
        tickMarkFormatter: (time: Time) =>
          new Intl.DateTimeFormat(undefined, {
            hour: "2-digit",
            minute: "2-digit",
          }).format(new Date(Number(time) * 1000)),
      },
    });

    const macd = chart.addSeries(LineSeries, {
      color: MACD_SERIES[0].color,
      lineWidth: 2,
      priceLineColor: MACD_SERIES[0].color,
      priceLineVisible: true,
      lastValueVisible: true,
    });
    const signal = chart.addSeries(LineSeries, {
      color: MACD_SERIES[1].color,
      lineWidth: 2,
      priceLineColor: MACD_SERIES[1].color,
      priceLineVisible: true,
      lastValueVisible: true,
    });
    const histogram = chart.addSeries(HistogramSeries, {
      priceLineVisible: false,
      base: 0,
      color: MACD_SERIES[2].color,
    });
    seriesRef.current = { chart, macd, signal, histogram };

    const updateSeriesLabelPositions = () => {
      const timeScale = chart.timeScale();
      const visibleRange = timeScale.getVisibleRange();
      const from = visibleRange ? Number(visibleRange.from) : null;
      const to = visibleRange ? Number(visibleRange.to) : null;
      const maxX = Math.max(8, container.clientWidth - 160);
      const maxY = Math.max(6, container.clientHeight - 22);
      const nextPositions = MACD_SERIES.flatMap((item) => {
        if (!visibilityRef.current[item.key]) return [];

        const isVisibleTime = (time: UTCTimestamp) => {
          const timestamp = Number(time);
          return (
            (from === null || timestamp >= from) &&
            (to === null || timestamp <= to)
          );
        };
        const point =
          item.key === "macd"
            ? (macdDataRef.current.find(({ time }) => isVisibleTime(time)) ??
              macdDataRef.current[0])
            : item.key === "signal"
              ? (signalDataRef.current.find(({ time }) =>
                  isVisibleTime(time)
                ) ?? signalDataRef.current[0])
              : (histogramDataRef.current.find(({ time }) =>
                  isVisibleTime(time)
                ) ?? histogramDataRef.current[0]);

        if (!point) return [];
        const x = timeScale.timeToCoordinate(point.time);
        const y =
          item.key === "macd"
            ? macd.priceToCoordinate(point.value)
            : item.key === "signal"
              ? signal.priceToCoordinate(point.value)
              : histogram.priceToCoordinate(point.value);
        if (x === null || y === null) return [];

        return [
          {
            key: item.key,
            label: item.label,
            color: item.color,
            x: Math.min(maxX, Math.max(8, x + 8)),
            y: Math.min(maxY, Math.max(6, y - 10)),
          },
        ];
      });
      setSeriesLabelPositions(nextPositions);
    };
    updateSeriesLabelPositionsRef.current = updateSeriesLabelPositions;
    const handleChartInteraction = () => {
      hasUserInteractedWithChartRef.current = true;
    };
    const handleVisibleTimeRangeChange = () => {
      if (
        Math.abs(chart.timeScale().scrollPosition()) <= MANUAL_SCROLL_THRESHOLD
      ) {
        hasUserInteractedWithChartRef.current = false;
      }
      updateSeriesLabelPositions();
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
    chart.timeScale().subscribeSizeChange(updateSeriesLabelPositions);

    const handleCrosshairMove = (
      param: import("lightweight-charts").MouseEventParams<Time>
    ) => {
      if (param.time === undefined || !param.point) {
        setHoveredPoint(null);
        return;
      }

      const container = chartRef.current;
      if (container) {
        const tooltip = tooltipRef.current;
        const tooltipWidth = tooltip?.offsetWidth ?? 190;
        const tooltipHeight = tooltip?.offsetHeight ?? 76;
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
        setTooltipPosition({ x, y });
      }

      setHoveredPoint(pointsByTimeRef.current.get(Number(param.time)) ?? null);
    };
    chart.subscribeCrosshairMove(handleCrosshairMove);

    const themeObserver = new MutationObserver(() => {
      const gridColor = document.documentElement.classList.contains("dark")
        ? DARK_GRID_COLOR
        : LIGHT_GRID_COLOR;
      chart.applyOptions({
        grid: {
          vertLines: { color: gridColor, style: 2 },
          horzLines: { color: gridColor, style: 2 },
        },
        rightPriceScale: { borderVisible: isDarkMode, borderColor: gridColor },
        timeScale: { borderVisible: isDarkMode, borderColor: gridColor },
      });
    });
    themeObserver.observe(document.documentElement, {
      attributes: true,
      attributeFilter: ["class"],
    });

    return () => {
      themeObserver.disconnect();
      container.removeEventListener("pointerdown", handleChartInteraction);
      container.removeEventListener("wheel", handleChartInteraction);
      container.removeEventListener("touchstart", handleChartInteraction);
      chart
        .timeScale()
        .unsubscribeVisibleTimeRangeChange(handleVisibleTimeRangeChange);
      chart.timeScale().unsubscribeSizeChange(updateSeriesLabelPositions);
      updateSeriesLabelPositionsRef.current = null;
      setSeriesLabelPositions([]);
      chart.unsubscribeCrosshairMove(handleCrosshairMove);
      seriesRef.current = null;
      chart.remove();
    };
  }, []);

  React.useEffect(() => {
    const chart = seriesRef.current?.chart;
    if (!chart) return;
    const frame = requestAnimationFrame(() => chart.resize(
      chartRef.current?.clientWidth ?? 0,
      chartRef.current?.clientHeight ?? 0
    ));
    return () => cancelAnimationFrame(frame);
  });

  React.useEffect(() => {
    const series = seriesRef.current;
    if (!series) return;
    const timeScale = series.chart.timeScale();
    const shouldPreserveVisibleRange =
      hasUserInteractedWithChartRef.current &&
      Math.abs(timeScale.scrollPosition()) > MANUAL_SCROLL_THRESHOLD;
    const visibleLogicalRange = shouldPreserveVisibleRange
      ? timeScale.getVisibleLogicalRange()
      : null;
    const macdData: LineData<UTCTimestamp>[] = points.map(
      ({ time, value }) => ({ time, value })
    );
    const signalData: LineData<UTCTimestamp>[] = points.map(
      ({ time, signal }) => ({ time, value: signal })
    );
    const histogramData: HistogramData<UTCTimestamp>[] = points.map(
      ({ time, histogram: value }) => ({
        time,
        value,
        color:
          value >= 0 ? "rgba(22, 163, 74, 0.55)" : "rgba(220, 38, 38, 0.55)",
      })
    );
    macdDataRef.current = macdData;
    signalDataRef.current = signalData;
    histogramDataRef.current = histogramData;
    pointsByTimeRef.current = new Map(
      points.map((point) => [point.time, point])
    );
    series.macd.setData(macdData);
    series.signal.setData(signalData);
    series.histogram.setData(histogramData);
    if (points.length > 0) {
      const container = chartRef.current;
      const visiblePointCount = Math.min(
        points.length,
        INITIAL_VISIBLE_POINT_LIMIT
      );
      const initialBarSpacing = Math.max(
        MIN_INITIAL_BAR_SPACING,
        Math.min(
          MAX_INITIAL_BAR_SPACING,
          (container?.clientWidth ?? 0) / visiblePointCount
        )
      );

      if (visibleLogicalRange) {
        timeScale.setVisibleLogicalRange(visibleLogicalRange);
      } else {
        timeScale.applyOptions({ barSpacing: initialBarSpacing });
        timeScale.scrollToPosition(0, false);
      }
    }
    updateSeriesLabelPositionsRef.current?.();
  }, [points]);

  return (
    <section className="flex min-h-64 flex-col" aria-label="MACD chart">
      <h3 className="mb-1 text-xs font-semibold text-muted-foreground">
        {points[0]?.name ?? "MACD"}
      </h3>
      <div
        className="mb-2 flex min-h-7 flex-wrap items-center gap-x-4 gap-y-2"
        aria-label="Chart series visibility"
      >
        {MACD_SERIES.map((item) => (
          <label
            key={item.key}
            className="flex min-w-0 cursor-pointer items-center gap-2 text-xs font-medium text-muted-foreground"
          >
            <Checkbox
              checked={visibility[item.key]}
              onCheckedChange={(checked) =>
                handleVisibilityChange(item.key, checked)
              }
              aria-label={`Show ${item.label} series`}
            />
            <span
              className="size-2.5 shrink-0 rounded-full"
              style={{ backgroundColor: item.color }}
              aria-hidden="true"
            />
            <span className="truncate">{item.label}</span>
          </label>
        ))}
      </div>
      <div className="relative min-h-0 flex-1">
        {hoveredPoint ? (
          <dl
            ref={tooltipRef}
            className="pointer-events-none absolute z-20 flex flex-col gap-y-1 rounded-md border bg-background/90 px-2 py-1 text-xs tabular-nums shadow-sm"
            aria-label="Hovered MACD values"
            style={{ left: tooltipPosition.x, top: tooltipPosition.y }}
          >
            <dt className="font-medium">
              {new Intl.DateTimeFormat(undefined, {
                year: "numeric",
                month: "2-digit",
                day: "2-digit",
                hour: "2-digit",
                minute: "2-digit",
                second: "2-digit",
              }).format(new Date(Number(hoveredPoint.time) * 1000))}
            </dt>
            <dt className="text-muted-foreground">{hoveredPoint.name}</dt>
            <div className="flex gap-1">
              <dt className="text-muted-foreground">MACD</dt>
              <dd>{hoveredPoint.value.toFixed(2)}</dd>
            </div>
            <div className="flex gap-1">
              <dt className="text-muted-foreground">Signal</dt>
              <dd>{hoveredPoint.signal.toFixed(2)}</dd>
            </div>
            <div className="flex gap-1">
              <dt className="text-muted-foreground">Hist</dt>
              <dd>{hoveredPoint.histogram.toFixed(2)}</dd>
            </div>
          </dl>
        ) : null}
        {seriesLabelPositions.map((series) => (
          <div
            key={series.key}
            className="pointer-events-none absolute z-10 flex max-w-36 items-center gap-1.5 rounded bg-background/85 px-1.5 py-0.5 text-[10px] font-medium leading-none text-foreground shadow-sm"
            style={{ left: series.x, top: series.y }}
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
    </section>
  );
}
