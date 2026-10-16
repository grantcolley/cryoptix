interface MacdSnapshotProps {
  name: string;
  value: number;
  signal: number;
  histogram: number;
}

export function MacdSnapshot({
  name,
  value,
  signal,
  histogram,
}: MacdSnapshotProps) {
  const valueColor =
    histogram > 0
      ? "bg-green-500 text-black"
      : histogram < 0
        ? "bg-red-600 text-white"
        : "bg-muted text-muted-foreground";

  return (
    <div className="flex h-5 shrink-0 items-stretch overflow-hidden rounded-md border text-sm leading-none">
      <dt className="flex items-center px-2 text-foreground-semimuted">
        {name}
      </dt>
      <dd className={`flex items-center border-l px-2 tabular-nums ${valueColor}`}>
        {value.toFixed(2)}
      </dd>
      <dd className={`flex items-center border-l px-2 tabular-nums ${valueColor}`}>
        {signal.toFixed(2)}
      </dd>
      <dd className={`flex items-center border-l px-2 tabular-nums ${valueColor}`}>
        {histogram.toFixed(2)}
      </dd>
    </div>
  );
}
