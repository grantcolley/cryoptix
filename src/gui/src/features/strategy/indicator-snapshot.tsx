interface IndicatorSnapshotProps {
  name: string;
  value: number;
}

export function IndicatorSnapshot({ name, value }: IndicatorSnapshotProps) {
  const valueColor =
    value < 0 || value > 100
      ? "bg-muted text-muted-foreground"
      : value < 25
        ? "bg-red-600 text-white"
        : value < 50
          ? "bg-orange-400 text-black"
          : value < 75
            ? "bg-green-500 text-black"
            : "bg-green-800 text-white";

  return (
    <div className="flex h-5 shrink-0 items-stretch overflow-hidden rounded-md border text-sm leading-none">
      <dt className="flex items-center px-2 text-foreground-semimuted">
        {name}
      </dt>
      <dd
        className={`flex items-center border-l px-2 tabular-nums ${valueColor}`}
      >
        {value.toFixed(2)}
      </dd>
    </div>
  );
}
