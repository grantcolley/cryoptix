import type { ComponentProps } from "react";
import { StrategyExecution } from "@/features/strategy/strategy-execution";
import { StrategyHeader } from "@/features/strategy/strategy-header";

interface StrategyToolbarProps {
  headerProps: ComponentProps<typeof StrategyHeader>;
  executionProps: ComponentProps<typeof StrategyExecution>;
  onConnectSubmit: ComponentProps<"form">["onSubmit"];
  connectError: string | null;
  notificationMessage: string | null;
}

export function StrategyToolbar({
  headerProps,
  executionProps,
  onConnectSubmit,
  connectError,
  notificationMessage,
}: StrategyToolbarProps) {
  const isHeaderVisible = headerProps.showStrategyRunning && !!headerProps.strategy;

  return (
    <div className="shrink-0 px-4 pt-4 pb-2">
      <div className="flex flex-wrap items-center gap-x-4 gap-y-2">
        <StrategyHeader {...headerProps} />
        <form
          className={`ml-auto flex w-full min-w-0 items-center gap-1 [&>span]:min-w-0 [&>span]:flex-1 ${
            isHeaderVisible ? "sm:w-auto sm:max-w-full sm:[&>span]:w-64" : ""
          }`}
          onSubmit={onConnectSubmit}
        >
          <StrategyExecution {...executionProps} />
        </form>
      </div>
      {connectError && (
        <p className="mt-2 text-sm text-destructive">{connectError}</p>
      )}
      {notificationMessage && (
        <p className="mt-2 text-sm text-muted-foreground">
          {notificationMessage}
        </p>
      )}
    </div>
  );
}
