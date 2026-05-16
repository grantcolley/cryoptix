import { STRATEGY_CONFIG } from "@/data/strategy-config";
import * as React from "react";
import { ExchangeLabels } from "@/features/api/schema/exchange";
import type { Strategy } from "@/features/api/schema/strategy-schema";
import StrategyForm from "@/features/strategy/strategy-form";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { Collapsible, CollapsibleContent } from "@/components/ui/collapsible";

interface StrategySelectProps {
  isOpen: boolean;
  showSelect: boolean;
  selectedStrategyId: string;
  strategy: Strategy | null;
  strategyFormVersion: number;
  onOpenChange: (open: boolean) => void;
  onStrategyChange: (value: string) => void;
  onStrategyFormChange: (nextStrategy: Strategy) => void;
}

export function StrategySelect({
  isOpen,
  showSelect,
  selectedStrategyId,
  strategy,
  strategyFormVersion,
  onOpenChange,
  onStrategyChange,
  onStrategyFormChange,
}: StrategySelectProps) {
  const [isStrategyOpen, setIsStrategyOpen] = React.useState(false);
  const [isSubscriptionOpen, setIsSubscriptionOpen] = React.useState(false);
  const [isBroadcastOpen, setIsBroadcastOpen] = React.useState(false);

  return (
    <Collapsible
      open={isOpen}
      onOpenChange={onOpenChange}
      className="group/collapsible grid auto-rows-min rounded-xl px-4 py-2"
    >
      <div className="flex items-center gap-1 py-2">
        {showSelect && (
          <Select
            value={selectedStrategyId}
            onValueChange={onStrategyChange}
            aria-label="Select a strategy"
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
        )}

        {strategy && !showSelect && (
          <div className="flex flex-row items-baseline gap-4">
            <h4 className="text-md text-foreground">{strategy.name}</h4>
            <h4 className="text-sm text-foreground-semimuted">
              {strategy.symbol}
            </h4>
            <h4 className="text-sm text-foreground-semimuted">
              {ExchangeLabels[strategy.exchange]}
            </h4>
          </div>
        )}
      </div>

      {strategy && (
        <CollapsibleContent className="flex flex-col gap-2 pt-2 pb-2">
          <StrategyForm
            key={`${strategy.strategyId}-${strategyFormVersion}`}
            defaultValues={strategy}
            showSubmitButton={false}
            isStrategyOpen={isStrategyOpen}
            isSubscriptionOpen={isSubscriptionOpen}
            isBroadcastOpen={isBroadcastOpen}
            onChange={onStrategyFormChange}
            onStrategyOpenChange={setIsStrategyOpen}
            onSubscriptionOpenChange={setIsSubscriptionOpen}
            onBroadcastOpenChange={setIsBroadcastOpen}
          />
        </CollapsibleContent>
      )}
    </Collapsible>
  );
}
