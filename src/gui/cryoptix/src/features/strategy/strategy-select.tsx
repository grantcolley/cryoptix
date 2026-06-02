import { STRATEGY_CONFIG } from "@/data/strategy-config";
import * as React from "react";
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
import { cn } from "@/lib/utils";

interface StrategySelectProps {
  isOpen: boolean;
  canSelectStrategy: boolean;
  showParametersOnly?: boolean;
  selectedStrategyId: string;
  strategy: Strategy | null;
  strategyFormVersion: number;
  onOpenChange: (open: boolean) => void;
  onStrategyChange: (value: string) => void;
  onStrategyFormChange: (nextStrategy: Strategy) => void;
}

export function StrategySelect({
  isOpen,
  canSelectStrategy,
  showParametersOnly = false,
  selectedStrategyId,
  strategy,
  strategyFormVersion,
  onOpenChange,
  onStrategyChange,
  onStrategyFormChange,
}: StrategySelectProps) {
  const [isStrategyOpen, setIsStrategyOpen] = React.useState(true);
  const [isSubscriptionOpen, setIsSubscriptionOpen] = React.useState(true);
  const [isParametersOpen, setIsParametersOpen] = React.useState(true);
  const [isBroadcastOpen, setIsBroadcastOpen] = React.useState(true);
  const hasVisibleContent = canSelectStrategy || (isOpen && strategy !== null);

  return (
    <Collapsible
      open={isOpen}
      onOpenChange={onOpenChange}
      className={cn(
        "group/collapsible grid auto-rows-min rounded-xl px-4",
        hasVisibleContent && "py-1"
      )}
    >
      {canSelectStrategy && (
        <div className="flex items-center gap-1 py-1">
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
        </div>
      )}

      {strategy && (
        <CollapsibleContent className="flex flex-col gap-2 py-1">
          <StrategyForm
            key={`${strategy.strategyId}-${strategyFormVersion}`}
            defaultValues={strategy}
            showSubmitButton={false}
            isCompact
            isStrategyOpen={isStrategyOpen}
            isReadOnly={!canSelectStrategy}
            showParametersOnly={showParametersOnly}
            isSubscriptionOpen={isSubscriptionOpen}
            isParametersOpen={isParametersOpen}
            isBroadcastOpen={isBroadcastOpen}
            onChange={onStrategyFormChange}
            onStrategyOpenChange={setIsStrategyOpen}
            onParametersOpenChange={setIsParametersOpen}
            onSubscriptionOpenChange={setIsSubscriptionOpen}
            onBroadcastOpenChange={setIsBroadcastOpen}
          />
        </CollapsibleContent>
      )}
    </Collapsible>
  );
}
