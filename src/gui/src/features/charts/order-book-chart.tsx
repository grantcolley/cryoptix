import type { OrderBook } from "@/features/api/schema/order-book-schema";
import type { OrderBookPrice } from "@/features/api/schema/order-book-price-schema";

function cumulativeLevels(levels: OrderBookPrice[], descending: boolean) {
  const quantities = new Map<number, number>();
  for (const { price, quantity } of levels) {
    if (Number.isFinite(price) && Number.isFinite(quantity) && quantity > 0) {
      quantities.set(price, (quantities.get(price) ?? 0) + quantity);
    }
  }
  let total = 0;
  return [...quantities.entries()]
    .sort(([a], [b]) => (descending ? b - a : a - b))
    .map(([price, quantity]) => ({ price, quantity: (total += quantity) }));
}

export function OrderBookChart({ orderBook }: { orderBook: OrderBook | null }) {
  const bids = cumulativeLevels(
    orderBook?.bids ?? (orderBook?.bestBid ? [orderBook.bestBid] : []),
    true
  );
  const asks = cumulativeLevels(
    orderBook?.asks ?? (orderBook?.bestAsk ? [orderBook.bestAsk] : []),
    false
  );
  if (!bids.length && !asks.length) {
    return (
      <div
        role="status"
        className="flex min-h-48 flex-1 items-center justify-center text-sm text-muted-foreground"
      >
        Waiting for order book data
      </div>
    );
  }

  const bestBid = bids[0]?.price;
  const bestAsk = asks[0]?.price;
  const midpoint =
    bestBid !== undefined && bestAsk !== undefined
      ? (bestBid + bestAsk) / 2
      : (bestBid ?? bestAsk)!;
  const extent = Math.max(
    ...[...bids, ...asks].map(({ price }) => Math.abs(price - midpoint)),
    Math.abs(midpoint) * 0.000001,
    0.000001
  );
  const maximum = Math.max(
    ...[...bids, ...asks].map(({ quantity }) => quantity)
  );
  const x = (price: number) =>
    60 + ((price - midpoint + extent) / (extent * 2)) * 680;
  const y = (quantity: number) => 250 - (quantity / maximum) * 220;
  const format = (value: number) =>
    value.toLocaleString(undefined, { maximumSignificantDigits: 8 });
  const formatAxis = (value: number) =>
    value.toLocaleString(undefined, {
      notation: "compact",
      maximumSignificantDigits: 5,
    });
  const path = (points: typeof bids) => {
    if (!points.length) return "";
    return (
      `M ${x(points[0].price)} 250 ` +
      points
        .map(({ price, quantity }) => `H ${x(price)} V ${y(quantity)}`)
        .join(" ")
    );
  };

  return (
    <div className="flex min-h-0 flex-1 flex-col gap-2">
      <div className="flex justify-between text-xs">
        <span className="text-green-600 dark:text-green-400">Bids</span>
        {/* <span className="text-muted-foreground">Cumulative amount</span> */}
        <span className="text-destructive">Asks</span>
      </div>
      <div className="relative min-h-32 flex-1 basis-0">
        <svg
          viewBox="0 0 800 300"
          role="img"
          aria-label="Cumulative order book depth by price, bids on the left and asks on the right"
          preserveAspectRatio="none"
          className="absolute inset-0 h-full w-full"
        >
          {[0, 0.5, 1].map((fraction) => (
            <g key={fraction} className="text-muted-foreground">
              <line
                x1="60"
                x2="740"
                y1={y(maximum * fraction)}
                y2={y(maximum * fraction)}
                stroke="currentColor"
                opacity="0.15"
              />
            </g>
          ))}
          <line
            x1="400"
            x2="400"
            y1="30"
            y2="250"
            stroke="currentColor"
            opacity="0.2"
            strokeDasharray="4 4"
          />
          {[
            { points: bids, color: "text-green-600 dark:text-green-400" },
            { points: asks, color: "text-destructive" },
          ].map(({ points, color }, index) =>
            points.length ? (
              <g key={index} className={color}>
                <path
                  d={`${path(points)} V 250 Z`}
                  fill="currentColor"
                  opacity="0.15"
                />
                <path
                  d={path(points)}
                  fill="none"
                  stroke="currentColor"
                  strokeWidth="2"
                />
                {points.map(({ price, quantity }) => (
                  <circle
                    key={price}
                    cx={x(price)}
                    cy={y(quantity)}
                    r="3"
                    fill="transparent"
                  >
                    <title>{`Price: ${format(price)}, cumulative amount: ${format(quantity)}`}</title>
                  </circle>
                ))}
              </g>
            ) : null
          )}
        </svg>
        {[0, 0.5, 1].map((fraction) => (
          <span
            key={fraction}
            className="absolute left-0 -translate-y-1/2 rounded-sm bg-card/90 px-0.5 text-[9px] leading-none text-muted-foreground tabular-nums"
            style={{ top: `${y(maximum * fraction) / 3}%` }}
            title={`Cumulative amount: ${format(maximum * fraction)}`}
          >
            {formatAxis(maximum * fraction)}
          </span>
        ))}
        <div className="absolute inset-x-0 bottom-0 flex justify-between gap-1 text-[9px] leading-none text-muted-foreground tabular-nums">
          {[-1, 0, 1].map((offset) => (
            <span
              key={offset}
              title={`Price: ${format(midpoint + extent * offset)}`}
            >
              {formatAxis(midpoint + extent * offset)}
            </span>
          ))}
        </div>
      </div>
    </div>
  );
}
