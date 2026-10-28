import type { OrderBook as OrderBookData } from "@/features/api/schema/order-book-schema";
import type { OrderBookPrice } from "@/features/api/schema/order-book-price-schema";

type OrderBookProps = {
  orderBook: OrderBookData | null;
  price: string | null;
  priceClassName?: string;
  levels?: number;
};

export function OrderBook({
  orderBook,
  price,
  priceClassName,
  levels = 10,
}: OrderBookProps) {
  const asks = [
    ...(orderBook?.asks ?? (orderBook?.bestAsk ? [orderBook.bestAsk] : [])),
  ]
    .sort((a, b) => a.price - b.price)
    .slice(0, levels)
    .reverse();
  const bids = [
    ...(orderBook?.bids ?? (orderBook?.bestBid ? [orderBook.bestBid] : [])),
  ]
    .sort((a, b) => b.price - a.price)
    .slice(0, levels);

  const renderSide = (side: "Ask" | "Bid", rows: OrderBookPrice[]) => (
    <table
      className="w-full text-right text-sm tabular-nums"
      aria-label={`${side}s`}
    >
      <thead className="text-xs text-muted-foreground">
        <tr>
          <th scope="col" className="pb-2 text-left font-normal">
            {side} price
          </th>
          <th scope="col" className="pb-2 font-normal">
            Amount
          </th>
        </tr>
      </thead>
      <tbody>
        {rows.map((row, index) => (
          <tr key={`${row.price}-${index}`}>
            <td
              className={`py-0.5 text-left ${side === "Ask" ? "text-destructive" : "text-green-600 dark:text-green-400"}`}
            >
              {row.price}
            </td>
            <td className="py-0.5">{row.quantity}</td>
          </tr>
        ))}
        {rows.length === 0 ? (
          <tr>
            <td
              colSpan={2}
              className="py-4 text-center text-xs text-muted-foreground"
            >
              Waiting for {side.toLowerCase()}s
            </td>
          </tr>
        ) : null}
      </tbody>
    </table>
  );

  return (
    <div className="flex w-full flex-col gap-3">
      <h3 className="text-sm font-medium">Order book</h3>
      {renderSide("Ask", asks)}
      <p
        aria-label="Current price"
        className={`border-y py-3 text-lg tabular-nums ${priceClassName ?? ""}`}
      >
        {price ?? "—"}
      </p>
      {renderSide("Bid", bids)}
    </div>
  );
}
