import * as React from "react";
import StrategyForm from "@/features/strategy/strategy-form";
import { STRATEGY_CONFIG } from "@/data/strategy-config";
import { Label } from "@/components/ui/label";
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
    <div className="flex flex-1 flex-col gap-4 p-4 pt-4">
      <div className="flex-1 rounded-xl bg-muted/50 md:min-h-min">
        <div>
          <Collapsible
            open={isOpen}
            onOpenChange={setIsOpen}
            className="group/collapsible grid auto-rows-min rounded-xl gap-4 p-4"
          >
            <div className="flex items-center justify-between gap-4">
              <div className="flex items-center gap-4">
                <div className="flex items-center gap-2">
                  <Select
                    value={selectedStrategyId}
                    onValueChange={handleStrategyChange}
                  >
                    <SelectTrigger className="w-[200px]">
                      <SelectValue placeholder="Select a strategy" />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value="__none__">No strategy</SelectItem>
                      {STRATEGY_CONFIG.map((s) => (
                        <SelectItem
                          key={s.strategyId}
                          value={String(s.strategyId)}
                        >
                          {s.name}
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                </div>

                <div className="flex items-center gap-2">
                  <Label>Server Url</Label>
                  <input
                    type="text"
                    className="rounded-md border border-input bg-background px-3 py-2 text-sm ring-offset-background file:border-0 file:bg-transparent file:text-sm file:font-medium placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-50"
                  />
                  <Button
                    variant="outline"
                    size="icon"
                    aria-label="Connect to server"
                  >
                    <Icon
                      icon={icons.plug}
                      className="transition-transform duration-200 rotate-90"
                    />
                  </Button>
                </div>
              </div>

              {strategy && (
                <CollapsibleTrigger asChild>
                  <Button
                    variant="outline"
                    size="icon"
                    aria-label="Toggle details"
                  >
                    <Icon
                      icon={icons.cheveronRight}
                      className="transition-transform duration-200 group-data-[state=open]/collapsible:rotate-90"
                    />
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
        </div>

        <div className="min-h-[100vh] flex-1 rounded-xl md:min-h-min px-4">
          Live Charts...
        </div>
      </div>
    </div>
  );
}

export default StrategyPage;
