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

type MacdPoint = {
  time: UTCTimestamp;
  value: number;
  signal: number;
  histogram: number;
};

const LIGHT_GRID_COLOR = "#e2e8f0";
const DARK_GRID_COLOR = "#273449";

function toMacdPoints(indicators: Indicators[]): MacdPoint[] {
  const points = new Map<string, MacdPoint>();

  for (const indicator of indicators) {
    const time = Math.floor(indicator.timestampUtc.getTime() / 1000);
    for (const macd of Object.values(indicator.macds)) {
      points.set(String(time), {
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
  const seriesRef = React.useRef<{
    chart: IChartApi;
    macd: ISeriesApi<"Line">;
    signal: ISeriesApi<"Line">;
    histogram: ISeriesApi<"Histogram">;
  } | null>(null);
  const points = React.useMemo(() => toMacdPoints(indicators), [indicators]);

  React.useEffect(() => {
    const container = chartRef.current;
    if (!container) return;

    const chart = createChart(container, {
      autoSize: true,
      layout: {
        background: { type: ColorType.Solid, color: "transparent" },
        textColor: "#64748b",
        attributionLogo: false,
      },
      grid: {
        vertLines: { color: LIGHT_GRID_COLOR, style: 2 },
        horzLines: { color: LIGHT_GRID_COLOR, style: 2 },
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
      color: "#2563eb",
      lineWidth: 2,
      title: "MACD",
      priceLineVisible: false,
    });
    const signal = chart.addSeries(LineSeries, {
      color: "#f97316",
      lineWidth: 2,
      title: "Signal",
      priceLineVisible: false,
    });
    const histogram = chart.addSeries(HistogramSeries, {
      title: "Histogram",
      priceLineVisible: false,
      base: 0,
    });
    seriesRef.current = { chart, macd, signal, histogram };

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
      seriesRef.current = null;
      chart.remove();
    };
  }, []);

  React.useEffect(() => {
    const series = seriesRef.current;
    if (!series) return;
    const macdData: LineData<UTCTimestamp>[] = points.map(({ time, value }) => ({ time, value }));
    const signalData: LineData<UTCTimestamp>[] = points.map(({ time, signal }) => ({ time, value: signal }));
    const histogramData: HistogramData<UTCTimestamp>[] = points.map(({ time, histogram: value }) => ({
      time,
      value,
      color: value >= 0 ? "rgba(22, 163, 74, 0.55)" : "rgba(220, 38, 38, 0.55)",
    }));
    series.macd.setData(macdData);
    series.signal.setData(signalData);
    series.histogram.setData(histogramData);
    if (points.length > 0) series.chart.timeScale().fitContent();
  }, [points]);

  return (
    <section className="flex min-h-64 flex-col" aria-label="MACD chart">
      <div className="mb-2 flex items-center gap-4 text-xs text-muted-foreground">
        <span className="text-blue-600">MACD</span>
        <span className="text-orange-500">Signal</span>
        <span>Histogram</span>
      </div>
      <div ref={chartRef} className="min-h-0 flex-1" />
    </section>
  );
}
