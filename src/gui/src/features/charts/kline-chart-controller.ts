import type { MarketDataSnapshot } from "@/features/api/messages/market-data-snapshot-schema";
import type { Kline } from "@/features/api/schema/kline-schema";
import type { Indicators } from "@/features/api/schema/indicators-schema";
import type { Signal } from "@/features/api/schema/signal-schema";

type ChartSnapshot = Pick<
  MarketDataSnapshot,
  "klines" | "indicators" | "signals"
>;

/** Synchronous commands retain every notification, even before the chart mounts. */
export interface KlineChartController {
  replaceSnapshot(snapshot: ChartSnapshot): void;
  applyKline(kline: Kline): void;
  applyIndicator(indicator: Indicators): void;
  applySignal(signal: Signal): void;
  getKline(time: number): Kline | undefined;
  reset(): void;
}

type KlineChartTarget = Omit<KlineChartController, "getKline">;

interface RetainedChart {
  klines: Map<number, Kline>;
  indicators: Map<string, Map<number, Indicators>>;
  signals: Signal[];
  target?: KlineChartTarget;
}

const charts = new WeakMap<KlineChartController, RetainedChart>();
const timestamp = (date: Date) => Math.floor(date.getTime() / 1000);

function retainIndicator(state: RetainedChart, indicator: Indicators) {
  for (const item of indicator.series) {
    const points =
      state.indicators.get(item.key) ?? new Map<number, Indicators>();
    points.set(timestamp(indicator.timestampUtc), {
      ...indicator,
      series: [item],
      snapshots: [],
      macds: {},
    });
    state.indicators.set(item.key, points);
  }
}

function retainedSnapshot(state: RetainedChart): ChartSnapshot {
  return {
    klines: [...state.klines.values()],
    indicators: [...state.indicators.values()].flatMap((points) => [
      ...points.values(),
    ]),
    signals: [...state.signals],
  };
}

export function createKlineChartController(): KlineChartController {
  const state: RetainedChart = {
    klines: new Map(),
    indicators: new Map(),
    signals: [],
  };
  const controller: KlineChartController = {
    replaceSnapshot(snapshot) {
      state.klines.clear();
      state.indicators.clear();
      for (const kline of snapshot.klines) {
        state.klines.set(timestamp(kline.openTime), kline);
      }
      for (const indicator of snapshot.indicators)
        retainIndicator(state, indicator);
      state.signals = [...snapshot.signals];
      state.target?.replaceSnapshot(snapshot);
    },
    applyKline(kline) {
      state.klines.set(timestamp(kline.openTime), kline);
      state.target?.applyKline(kline);
    },
    applyIndicator(indicator) {
      retainIndicator(state, indicator);
      state.target?.applyIndicator(indicator);
    },
    applySignal(signal) {
      state.signals.push(signal);
      state.target?.applySignal(signal);
    },
    getKline(time) {
      return state.klines.get(time);
    },
    reset() {
      state.klines.clear();
      state.indicators.clear();
      state.signals = [];
      state.target?.reset();
    },
  };
  charts.set(controller, state);
  return controller;
}

/** Internal mount adapter; StrategyPage only sees the command interface. */
export function attachKlineChart(
  controller: KlineChartController,
  target: KlineChartTarget
): () => void {
  const state = charts.get(controller);
  if (!state)
    throw new Error("Use createKlineChartController to create the controller");
  if (state.target)
    throw new Error("A controller can only attach to one KlineChart");
  target.replaceSnapshot(retainedSnapshot(state));
  state.target = target;
  return () => {
    if (state.target === target) state.target = undefined;
  };
}
