import StrategyForm from "@/features/strategy/strategy-form";
import { STRATEGY_CONFIG } from "@/data/strategy-config";

export function StrategyPage() {
  const strategy = STRATEGY_CONFIG[0];
  return (
    <div className="flex flex-1 flex-col gap-4 p-4 pt-4">
      <div className="grid auto-rows-min rounded-xl  p-4">
        <StrategyForm
          defaultValues={strategy}
          submitLabel="Update strategy"
          onSubmit={(updated) => {
            console.log(updated);
          }}
        />
      </div>
      <div className="min-h-[100vh] flex-1 rounded-xl bg-muted/50 md:min-h-min">
        Hello World
      </div>
    </div>
  );
}

export default StrategyPage;
