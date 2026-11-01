import type { OrderBook as OrderBookData } from "@/features/api/schema/order-book-schema";
import type { OrderBookPrice } from "@/features/api/schema/order-book-price-schema";
import type { Symbol } from "@/features/api/schema/symbol-schema";

type OrderBookProps = {
  orderBook: OrderBookData | null;
  symbol: Symbol | null;
  price: string | null;
  priceClassName?: string;
  levels?: number;
};

export function OrderBook({
  orderBook,
  symbol,
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
      className="w-full text-right text-[10px] tabular-nums"
      aria-label={`${side}s`}
    >
      <thead className="text-muted-foreground">
        <tr>
          <th scope="col" className="pb-2 text-left font-normal">
            {side} price{symbol?.quoteAsset ? ` (${symbol.quoteAsset})` : ""}
          </th>
          <th scope="col" className="pb-2 font-normal">
            Amount{symbol?.baseAsset ? ` (${symbol.baseAsset})` : ""}
          </th>
          <th scope="col" className="pb-2 font-normal">
            Total{symbol?.quoteAsset ? ` (${symbol.quoteAsset})` : ""}
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
            <td className="py-0.5">
              {(row.price * row.quantity).toLocaleString(undefined, {
                maximumFractionDigits: symbol?.quoteAssetPrecision ?? 8,
              })}
            </td>
          </tr>
        ))}
        {rows.length === 0 ? (
          <tr>
            <td
              colSpan={3}
              className="py-4 text-center text-muted-foreground"
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
      {renderSide("Ask", asks)}
      <p
        aria-label="Current price"
        className={`border-y py-3 text-xs tabular-nums ${priceClassName ?? ""}`}
      >
        {price ?? "—"}
      </p>
      {renderSide("Bid", bids)}
    </div>
  );
}
