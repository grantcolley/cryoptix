import * as React from "react";
import {
  ColorType,
  CrosshairMode,
  LineSeries,
  createChart,
  type IChartApi,
  type ISeriesApi,
  type LineData,
  type Time,
  type UTCTimestamp,
} from "lightweight-charts";
import type { Indicators } from "@/features/api/schema/indicators-schema";
import { Checkbox } from "@/components/ui/checkbox";

type RsiPoint = {
  time: UTCTimestamp;
  values: Record<string, number>;
};

const LIGHT_GRID_COLOR = "#e2e8f0";
const DARK_GRID_COLOR = "#273449";
const INITIAL_VISIBLE_POINT_LIMIT = 120;
const MIN_INITIAL_BAR_SPACING = 3;
const MAX_INITIAL_BAR_SPACING = 12;
const MANUAL_SCROLL_THRESHOLD = 0.5;
const RSI_COLORS = ["#2563eb", "#f97316", "#16a34a", "#a855f7", "#db2777"];

type RsiSeries = { key: string; label: string; color: string };
type RsiSeriesLabelPosition = RsiSeries & { x: number; y: number };

function toRsiPoints(indicators: Indicators[]): RsiPoint[] {
  const points = new Map<number, RsiPoint>();

  for (const indicator of indicators) {
    const time = Math.floor(indicator.timestampUtc.getTime() / 1000);
    const point = points.get(time) ?? {
      time: time as UTCTimestamp,
      values: {},
    };
    for (const rsi of indicator.rsis) point.values[rsi.key] = rsi.value;
    points.set(time, point);
  }

  return [...points.values()].sort((a, b) => a.time - b.time);
}

export function RsiChart({ indicators }: { indicators: Indicators[] }) {
  const chartRef = React.useRef<HTMLDivElement>(null);
  const tooltipRef = React.useRef<HTMLDListElement>(null);
  const [seriesLabelPositions, setSeriesLabelPositions] = React.useState<
    RsiSeriesLabelPosition[]
  >([]);
  const pointsByTimeRef = React.useRef(new Map<number, RsiPoint>());
  const [hoveredPoint, setHoveredPoint] = React.useState<RsiPoint | null>(null);
  const [tooltipPosition, setTooltipPosition] = React.useState({ x: 8, y: 8 });
  const seriesRef = React.useRef<{
    chart: IChartApi;
    lines: Map<string, ISeriesApi<"Line">>;
  } | null>(null);
  const points = React.useMemo(() => toRsiPoints(indicators), [indicators]);
  const seriesDataRef = React.useRef(
    new Map<string, LineData<UTCTimestamp>[]>()
  );
  const [visibility, setVisibility] = React.useState<Record<string, boolean>>(
    {}
  );
  const visibilityRef = React.useRef(visibility);
  const updateSeriesLabelPositionsRef = React.useRef<(() => void) | null>(null);
  const hasUserInteractedWithChartRef = React.useRef(false);
  const rsiSeries = React.useMemo<RsiSeries[]>(() => {
    const keys = [
      ...new Set(points.flatMap((point) => Object.keys(point.values))),
    ];
    return keys.map((key, index) => ({
      key,
      label: key,
      color: RSI_COLORS[index % RSI_COLORS.length],
    }));
  }, [points]);
  const rsiSeriesRef = React.useRef(rsiSeries);

  React.useEffect(() => {
    rsiSeriesRef.current = rsiSeries;
  }, [rsiSeries]);

  React.useEffect(() => {
    visibilityRef.current = visibility;
  }, [visibility]);

  const handleVisibilityChange = (
    key: string,
    checked: boolean | "indeterminate"
  ) => {
    const visible = checked === true;
    visibilityRef.current = { ...visibilityRef.current, [key]: visible };
    setVisibility((current) => ({ ...current, [key]: visible }));
    seriesRef.current?.lines.get(key)?.applyOptions({ visible });
    updateSeriesLabelPositionsRef.current?.();
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
    const lines = new Map<string, ISeriesApi<"Line">>();
    seriesRef.current = { chart, lines };

    const updateSeriesLabelPositions = () => {
      const timeScale = chart.timeScale();
      const visibleRange = timeScale.getVisibleRange();
      const from = visibleRange ? Number(visibleRange.from) : null;
      const to = visibleRange ? Number(visibleRange.to) : null;
      const maxX = Math.max(8, container.clientWidth - 160);
      const maxY = Math.max(6, container.clientHeight - 22);
      const nextPositions = rsiSeriesRef.current.flatMap((item) => {
        if (visibilityRef.current[item.key] === false) return [];
        const point =
          seriesDataRef.current.get(item.key)?.find(({ time }) => {
            const timestamp = Number(time);
            return (
              (from === null || timestamp >= from) &&
              (to === null || timestamp <= to)
            );
          }) ?? seriesDataRef.current.get(item.key)?.[0];
        const line = lines.get(item.key);
        if (!point || !line) return [];
        const x = timeScale.timeToCoordinate(point.time);
        const y = line.priceToCoordinate(point.value);
        if (x === null || y === null) return [];
        return [
          {
            ...item,
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
      const currentContainer = chartRef.current;
      if (currentContainer) {
        const tooltip = tooltipRef.current;
        const tooltipWidth = tooltip?.offsetWidth ?? 190;
        const tooltipHeight = tooltip?.offsetHeight ?? 76;
        const gap = 12;
        const x = Math.min(
          Math.max(4, param.point.x + gap),
          Math.max(4, currentContainer.clientWidth - tooltipWidth - 4)
        );
        const belowY = param.point.y + gap;
        const y = Math.min(
          Math.max(
            4,
            belowY + tooltipHeight <= currentContainer.clientHeight
              ? belowY
              : param.point.y - tooltipHeight - gap
          ),
          Math.max(4, currentContainer.clientHeight - tooltipHeight - 4)
        );
        setTooltipPosition({ x, y });
      }
      setHoveredPoint(pointsByTimeRef.current.get(Number(param.time)) ?? null);
    };
    chart.subscribeCrosshairMove(handleCrosshairMove);

    const themeObserver = new MutationObserver(() => {
      const nextGridColor = document.documentElement.classList.contains("dark")
        ? DARK_GRID_COLOR
        : LIGHT_GRID_COLOR;
      chart.applyOptions({
        grid: {
          vertLines: { color: nextGridColor, style: 2 },
          horzLines: { color: nextGridColor, style: 2 },
        },
        rightPriceScale: {
          borderVisible: isDarkMode,
          borderColor: nextGridColor,
        },
        timeScale: { borderVisible: isDarkMode, borderColor: nextGridColor },
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
    const frame = requestAnimationFrame(() =>
      chart.resize(
        chartRef.current?.clientWidth ?? 0,
        chartRef.current?.clientHeight ?? 0
      )
    );
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
    const activeKeys = new Set(rsiSeries.map(({ key }) => key));
    for (const [key, line] of series.lines) {
      if (!activeKeys.has(key)) {
        series.chart.removeSeries(line);
        series.lines.delete(key);
      }
    }
    for (const [index, item] of rsiSeries.entries()) {
      let line = series.lines.get(item.key);
      if (!line) {
        line = series.chart.addSeries(LineSeries, {
          color: item.color,
          lineWidth: 2,
          priceLineColor: item.color,
          priceLineVisible: true,
          lastValueVisible: true,
          visible: visibilityRef.current[item.key] ?? true,
        });
        series.lines.set(item.key, line);
      } else {
        line.applyOptions({
          color: item.color,
          priceLineColor: item.color,
          visible: visibilityRef.current[item.key] ?? true,
        });
      }
      const data: LineData<UTCTimestamp>[] = points.flatMap((point) =>
        point.values[item.key] === undefined
          ? []
          : [{ time: point.time, value: point.values[item.key] }]
      );
      seriesDataRef.current.set(item.key, data);
      line.setData(data);
      void index;
    }
    for (const key of [...seriesDataRef.current.keys()]) {
      if (!activeKeys.has(key)) seriesDataRef.current.delete(key);
    }
    pointsByTimeRef.current = new Map(
      points.map((point) => [point.time, point])
    );
    const chartContainer = chartRef.current;
    if (points.length > 0) {
      const visiblePointCount = Math.min(
        points.length,
        INITIAL_VISIBLE_POINT_LIMIT
      );
      const initialBarSpacing = Math.max(
        MIN_INITIAL_BAR_SPACING,
        Math.min(
          MAX_INITIAL_BAR_SPACING,
          (chartContainer?.clientWidth ?? 0) / visiblePointCount
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
  }, [points, rsiSeries]);

  return (
    <section className="flex min-h-64 flex-col" aria-label="RSI chart">
      <h3 className="mb-1 text-xs font-semibold text-muted-foreground">RSI</h3>
      <div
        className="mb-2 flex min-h-7 flex-wrap items-center gap-x-4 gap-y-2"
        aria-label="Chart series visibility"
      >
        {rsiSeries.map((item) => (
          <label
            key={item.key}
            className="flex min-w-0 cursor-pointer items-center gap-2 text-xs font-medium text-muted-foreground"
          >
            <Checkbox
              checked={visibility[item.key] ?? true}
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
            aria-label="Hovered RSI values"
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
            {rsiSeries.flatMap((item) =>
              hoveredPoint.values[item.key] === undefined ? (
                []
              ) : (
                <div key={item.key} className="flex gap-1">
                  <dt className="text-muted-foreground">{item.label}</dt>
                  <dd>{hoveredPoint.values[item.key].toFixed(2)}</dd>
                </div>
              )
            )}
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
