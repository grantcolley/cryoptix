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
import { Drawer } from "@base-ui/react/drawer";
import { Button } from "@/components/ui/button";
import { Icon } from "@/components/icon/icon";
import { icons } from "@/components/icon/icons";
import { ContentOverlayContext } from "@/providers/content-overlay-context";

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
  const contentOverlayContainer = React.useContext(ContentOverlayContext);
  const [isStrategyOpen, setIsStrategyOpen] = React.useState(true);
  const [isSubscriptionOpen, setIsSubscriptionOpen] = React.useState(true);
  const [isParametersOpen, setIsParametersOpen] = React.useState(true);
  const [isBroadcastOpen, setIsBroadcastOpen] = React.useState(true);
  const hasVisibleContent = canSelectStrategy || (isOpen && strategy !== null);
  const useDrawer = !canSelectStrategy;
  const drawerTriggerId = React.useRef("btnStrategyParameters");

  React.useEffect(() => {
    if (isOpen) {
      drawerTriggerId.current = showParametersOnly
        ? "btnStrategyParameters"
        : "btnStrategyConfig";
    }
  }, [isOpen, showParametersOnly]);

  const strategyForm = strategy ? (
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
  ) : null;

  return (
    <Collapsible
      open={isOpen && !useDrawer}
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

      {strategy && !useDrawer && (
        <CollapsibleContent className="flex flex-col gap-2 py-1">
          {strategyForm}
        </CollapsibleContent>
      )}
      <Drawer.Root
        open={
          isOpen &&
          useDrawer &&
          strategy !== null &&
          contentOverlayContainer !== null
        }
        onOpenChange={onOpenChange}
        swipeDirection="left"
        modal={false}
      >
        <Drawer.Portal
          container={contentOverlayContainer}
          className="pointer-events-auto"
        >
          <Drawer.Backdrop className="absolute inset-0 z-50 bg-black/40 transition-opacity duration-200 data-starting-style:opacity-0 data-ending-style:opacity-0" />
          <Drawer.Viewport className="absolute inset-0 z-50 flex justify-start">
            <Drawer.Popup
              finalFocus={() =>
                document.getElementById(drawerTriggerId.current)
              }
              className="flex h-full w-full max-w-md flex-col border-r bg-popover text-popover-foreground shadow-lg transition-transform duration-200 motion-reduce:transition-none data-swiping:transition-none data-starting-style:-translate-x-full data-ending-style:-translate-x-full"
              style={{
                transform: "translateX(var(--drawer-swipe-movement-x, 0px))",
              }}
            >
              <div className="flex items-start gap-4 border-b p-4">
                <div className="flex-1">
                  <Drawer.Title className="text-base font-medium">
                    {showParametersOnly
                      ? "Strategy parameters"
                      : "Strategy configuration"}
                  </Drawer.Title>
                  <Drawer.Description className="text-sm text-muted-foreground">
                    {strategy?.name} —{" "}
                    {showParametersOnly
                      ? "strategy logic inputs."
                      : "full strategy configuration."}
                  </Drawer.Description>
                </div>
                <Drawer.Close
                  render={
                    <Button
                      variant="ghost"
                      size="icon"
                      aria-label="Close strategy drawer"
                    />
                  }
                >
                  <Icon icon={icons.x} />
                </Drawer.Close>
              </div>
              <Drawer.Content
                className="min-h-0 flex-1 overflow-y-auto p-4 [&_[data-slot=field-set]]:w-full"
                data-base-ui-swipe-ignore
              >
                {useDrawer ? strategyForm : null}
              </Drawer.Content>
            </Drawer.Popup>
          </Drawer.Viewport>
        </Drawer.Portal>
      </Drawer.Root>
    </Collapsible>
  );
}
