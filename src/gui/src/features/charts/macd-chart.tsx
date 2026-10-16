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
const MACD_SERIES = [
  { key: "macd", label: "MACD", color: "#2563eb" },
  { key: "signal", label: "Signal", color: "#f97316" },
  { key: "histogram", label: "Histogram", color: "#64748b" },
] as const;

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
  const [visibility, setVisibility] = React.useState<Record<string, boolean>>({
    macd: true,
    signal: true,
    histogram: true,
  });

  const handleVisibilityChange = (
    key: string,
    checked: boolean | "indeterminate"
  ) => {
    const visible = checked === true;
    setVisibility((current) => ({ ...current, [key]: visible }));
    const series = seriesRef.current;
    if (!series) return;
    if (key === "macd" || key === "signal" || key === "histogram") {
      series[key].applyOptions({ visible });
    }
  };

  React.useEffect(() => {
    const container = chartRef.current;
    if (!container) return;

    const gridColor = document.documentElement.classList.contains("dark")
      ? DARK_GRID_COLOR
      : LIGHT_GRID_COLOR;
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
      timeScale: {
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
      title: "MACD",
      priceLineVisible: false,
    });
    const signal = chart.addSeries(LineSeries, {
      color: MACD_SERIES[1].color,
      lineWidth: 2,
      title: "Signal",
      priceLineVisible: false,
    });
    const histogram = chart.addSeries(HistogramSeries, {
      title: "Histogram",
      priceLineVisible: false,
      base: 0,
      color: MACD_SERIES[2].color,
    });
    seriesRef.current = { chart, macd, signal, histogram };

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
      });
    });
    themeObserver.observe(document.documentElement, {
      attributes: true,
      attributeFilter: ["class"],
    });

    return () => {
      themeObserver.disconnect();
      chart.unsubscribeCrosshairMove(handleCrosshairMove);
      seriesRef.current = null;
      chart.remove();
    };
  }, []);

  React.useEffect(() => {
    const series = seriesRef.current;
    if (!series) return;
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
    pointsByTimeRef.current = new Map(
      points.map((point) => [point.time, point])
    );
    series.macd.setData(macdData);
    series.signal.setData(signalData);
    series.histogram.setData(histogramData);
    if (points.length > 0) series.chart.timeScale().fitContent();
  }, [points]);

  return (
    <section className="flex min-h-64 flex-col" aria-label="MACD chart">
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
            <dt className="font-medium">{hoveredPoint.name}</dt>
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
        <div ref={chartRef} className="h-full w-full" />
      </div>
    </section>
  );
}
