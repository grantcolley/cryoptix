import * as React from "react";
import StrategyForm from "@/features/strategy/strategy-form";
import { STRATEGY_CONFIG } from "@/data/strategy-config";
import { Icon } from "@/components/icon/icon";
import { icons } from "@/components/icon/icons";
import { Button } from "@/components/ui/button";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import {
  Collapsible,
  CollapsibleContent,
  CollapsibleTrigger,
} from "@/components/ui/collapsible";

export function StrategyPage() {
  const [isOpen, setIsOpen] = React.useState(false);
  const [selectedStrategyId, setSelectedStrategyId] = React.useState("");

  const strategy =
    STRATEGY_CONFIG.find((s) => String(s.strategyId) === selectedStrategyId) ??
    null;

  const handleStrategyChange = (value: string) => {
    const nextStrategyId = value === "__none__" ? "" : value;

    setSelectedStrategyId(nextStrategyId);

    if (!nextStrategyId) {
      setIsOpen(false);
    }
  };

  return (
    <div className="flex flex-1 flex-col p-2">
      <div className="flex-1 rounded-xl bg-muted/50 md:min-h-min">
        <div className="flex items-center gap-1 px-4 pt-4 pb-2">
          <input
            type="text"
            className="rounded-md border border-input bg-background px-3 py-2 text-sm ring-offset-background file:border-0 file:bg-transparent file:text-sm file:font-medium placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-50"
          />
          <Button variant="outline" size="icon" aria-label="Connect to server">
            <Icon icon={icons.plug} className="rotate-90" />
          </Button>
        </div>

        <Collapsible
          open={isOpen}
          onOpenChange={setIsOpen}
          className="group/collapsible grid auto-rows-min rounded-xl px-4 py-2"
        >
          <div className="flex items-center gap-1">
            <Select
              value={selectedStrategyId}
              onValueChange={handleStrategyChange}
            >
              <SelectTrigger className="w-[255px]">
                <SelectValue placeholder="Select a strategy" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="__none__">No strategy</SelectItem>
                {STRATEGY_CONFIG.map((s) => (
                  <SelectItem key={s.strategyId} value={String(s.strategyId)}>
                    {s.name}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>

            {strategy && (
              <CollapsibleTrigger asChild>
                <Button
                  variant="outline"
                  size="icon"
                  aria-label="Toggle details"
                >
                  <Icon icon={isOpen ? icons.minus : icons.plus} />
                </Button>
              </CollapsibleTrigger>
            )}
          </div>

          {strategy && (
            <CollapsibleContent className="flex flex-col gap-2">
              <StrategyForm
                defaultValues={strategy}
                submitLabel="Update strategy"
                onSubmit={(updated) => {
                  console.log(updated);
                }}
              />
            </CollapsibleContent>
          )}
        </Collapsible>

        <div className="min-h-[100vh] flex-1 rounded-xl md:min-h-min px-4 py-2">
          Live Charts...
        </div>
      </div>
    </div>
  );
}

export default StrategyPage;
