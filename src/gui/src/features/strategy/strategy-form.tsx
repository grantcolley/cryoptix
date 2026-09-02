"use client";

import * as React from "react";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { useForm, useWatch } from "react-hook-form";
import { Button } from "@/components/ui/button";
import { FieldGroup, FieldSet } from "@/components/ui/field";
import {
  Collapsible,
  CollapsibleContent,
  CollapsibleTrigger,
} from "@/components/ui/collapsible";
import { Icon } from "@/components/icon/icon";
import { icons } from "@/components/icon/icons";
import { Exchange, ExchangeLabels } from "@/features/api/schema/exchange";
import {
  KlineInterval,
  KlineIntervalLabels,
} from "@/features/api/schema/kline-interval";
import {
  BoundedChannelFullMode,
  BoundedChannelFullModeLabels,
} from "@/features/api/schema/bounded-channel-full-mode";
import {
  StrategyEngineType,
  StrategyEngineTypeLabels,
} from "@/features/api/schema/strategy-engine-type";
import { MovingAverageSmoothingType } from "@/features/api/schema/moving-average-smothing-type";
import {
  StrategyProcessorType,
  StrategyProcessorTypeLabels,
} from "@/features/api/schema/strategy-processor-type";
import {
  StrategySchema,
  type Strategy,
} from "@/features/api/schema/strategy-schema";
import { MovingAveragePeriod } from "@/features/strategy/moving-average-period";
import {
  inputTextToNullable,
  nullableTextToInputValue,
} from "@/features/strategy/strategy-form-field-utils";
import {
  EnumSelectField,
  IntegerField,
  NullableIntegerField,
  TextAreaField,
  TextField,
} from "@/features/strategy/strategy-form-fields";
import {
  Tooltip,
  TooltipContent,
  TooltipTrigger,
} from "@/components/ui/tooltip";

import { enumToOptions } from "@/lib/enum-helper";
import { cn } from "@/lib/utils";

type StrategyFormValues = z.input<typeof StrategySchema>;

type StrategyFormProps = {
  defaultValues?: Partial<Strategy>;
  submitLabel?: string;
  showSubmitButton?: boolean;
  isReadOnly?: boolean;
  isCompact?: boolean;
  showParametersOnly?: boolean;
  isSubscriptionOpen?: boolean;
  isParametersOpen?: boolean;
  isStrategyOpen?: boolean;
  isBroadcastOpen?: boolean;
  onSubmit?: (strategy: Strategy) => void | Promise<void>;
  onChange?: (strategy: Strategy) => void;
  onSubscriptionOpenChange?: (open: boolean) => void;
  onParametersOpenChange?: (open: boolean) => void;
  onStrategyOpenChange?: (open: boolean) => void;
  onBroadcastOpenChange?: (open: boolean) => void;
};

const strategyProcessorTypeOptions = enumToOptions(
  StrategyProcessorType,
  StrategyProcessorTypeLabels
);
const strategyEngineTypeOptions = enumToOptions(
  StrategyEngineType,
  StrategyEngineTypeLabels
);
const exchangeOptions = enumToOptions(Exchange, ExchangeLabels);
const klineIntervalOptions = enumToOptions(KlineInterval, KlineIntervalLabels);
const boundedChannelFullModeOptions = enumToOptions(
  BoundedChannelFullMode,
  BoundedChannelFullModeLabels
);

const fallbackDefaultValues: Strategy = {
  strategyId: 0,
  name: "",
  description: null,
  symbol: null,
  strategyProcessorType: StrategyProcessorType.None,
  strategyEngineType: StrategyEngineType.None,
  exchange: Exchange.None,
  periods: {
    "9 EMA": {
      name: "9 EMA",
      value: 9,
      smoothingType: MovingAverageSmoothingType.Ema,
    },
    "21 EMA": {
      name: "21 EMA",
      value: 21,
      smoothingType: MovingAverageSmoothingType.Ema,
    },
    "50 EMA": {
      name: "50 EMA",
      value: 50,
      smoothingType: MovingAverageSmoothingType.Ema,
    },
  },
  klineInterval: KlineInterval.Minute,
  klineSeedSize: 1440,
  klineSeedLimit: 1000,
  orderBookLimit: 20,
  maxOrderBookAgeSeconds: 3,
  maxAccountAgeSeconds: 10,
  cacheMaxKlinesPerSeries: 5000,
  cacheMaxTradesPerSymbol: 10000,
  cacheMaxIndicatorsPerSeries: 5000,
  cacheMaxSignalsPerSeries: 5000,
  strategyProcessorMaxTradesPerPass: 256,
  subscriptionChannelKlineCapacity: 10000,
  subscriptionChannelKlineFullMode: BoundedChannelFullMode.DropOldest,
  subscriptionChannelTradeCapacity: 10000,
  subscriptionChannelTradeFullMode: BoundedChannelFullMode.DropOldest,
  marketEventDispatcherCapacity: 20000,
  marketEventDispatcherFullMode: BoundedChannelFullMode.DropOldest,
  klineBroadcastCapacity: 500,
  klineBroadcastFullMode: BoundedChannelFullMode.DropOldest,
  tradeBroadcastCapacity: 10000,
  tradeBroadcastFullMode: BoundedChannelFullMode.DropOldest,
  indicatorsBroadcastCapacity: 5000,
  indicatorsBroadcastFullMode: BoundedChannelFullMode.DropOldest,
  signalBroadcastCapacity: 5000,
  signalBroadcastFullMode: BoundedChannelFullMode.DropOldest,
};

function normalizeStrategyValues(values: StrategyFormValues): Strategy {
  return StrategySchema.parse({
    ...values,
    name: inputTextToNullable(nullableTextToInputValue(values.name)),
    description: inputTextToNullable(
      nullableTextToInputValue(values.description)
    ),
    symbol: inputTextToNullable(nullableTextToInputValue(values.symbol)),
  });
}

export function StrategyForm({
  defaultValues,
  submitLabel = "Update strategy",
  showSubmitButton = true,
  isReadOnly = false,
  isCompact = false,
  showParametersOnly = false,
  isSubscriptionOpen,
  isParametersOpen,
  isStrategyOpen,
  isBroadcastOpen,
  onSubmit,
  onChange,
  onSubscriptionOpenChange,
  onParametersOpenChange,
  onStrategyOpenChange,
  onBroadcastOpenChange,
}: StrategyFormProps) {
  const mergedDefaultValues = React.useMemo<Strategy>(
    () => ({
      ...fallbackDefaultValues,
      ...defaultValues,
    }),
    [defaultValues]
  );

  const form = useForm<StrategyFormValues, unknown, Strategy>({
    resolver: zodResolver(StrategySchema),
    defaultValues: mergedDefaultValues,
  });
  const periods = useWatch({
    control: form.control,
    name: "periods",
  });

  const [uncontrolledSubscriptionOpen, setUncontrolledSubscriptionOpen] =
    React.useState(false);
  const [uncontrolledStrategyOpen, setUncontrolledStrategyOpen] =
    React.useState(false);
  const [uncontrolledParametersOpen, setUncontrolledParametersOpen] =
    React.useState(false);
  const [uncontrolledBroadcastOpen, setUncontrolledBroadcastOpen] =
    React.useState(false);

  const subscriptionOpen = isSubscriptionOpen ?? uncontrolledSubscriptionOpen;
  const strategyOpen = isStrategyOpen ?? uncontrolledStrategyOpen;
  const parametersOpen = isParametersOpen ?? uncontrolledParametersOpen;
  const broadcastOpen = isBroadcastOpen ?? uncontrolledBroadcastOpen;

  const handleSubscriptionOpenChange = (open: boolean) => {
    setUncontrolledSubscriptionOpen(open);
    onSubscriptionOpenChange?.(open);
  };

  const handleStrategyOpenChange = (open: boolean) => {
    setUncontrolledStrategyOpen(open);
    onStrategyOpenChange?.(open);
  };

  const handleParametersOpenChange = (open: boolean) => {
    setUncontrolledParametersOpen(open);
    onParametersOpenChange?.(open);
  };

  const handleBroadcastOpenChange = (open: boolean) => {
    setUncontrolledBroadcastOpen(open);
    onBroadcastOpenChange?.(open);
  };

  const handleAddPeriod = () => {
    const current = form.getValues("periods") ?? {};

    const base = "New Period";
    let index = 1;
    let key = `${base} ${index}`;
    while (current[key]) {
      index += 1;
      key = `${base} ${index}`;
    }

    const period = {
      name: key,
      value: 9,
      smoothingType: MovingAverageSmoothingType.Sma,
    } as const;

    form.setValue("periods", { ...current, [key]: period });
  };

  React.useEffect(() => {
    if (!onChange) return;

    const subscription = form.watch((values) => {
      const parsed = StrategySchema.safeParse({
        ...values,
        name: inputTextToNullable(nullableTextToInputValue(values.name)),
        description: inputTextToNullable(
          nullableTextToInputValue(values.description)
        ),
        symbol: inputTextToNullable(nullableTextToInputValue(values.symbol)),
      });

      if (parsed.success) {
        onChange(parsed.data);
      }
    });

    return () => subscription.unsubscribe();
  }, [form, onChange]);

  async function handleSubmit(values: Strategy) {
    const normalizedValues = normalizeStrategyValues(values);

    await onSubmit?.(normalizedValues);
  }

  const renderParameterFields = (isHorizontal = false) => {
    const periodEntries = Object.entries(periods ?? {});

    return (
      <div className={cn("flex gap-3", isHorizontal ? "flex-row" : "flex-col")}>
        {!isReadOnly ? (
          <div className="flex justify-start">
            <Tooltip>
              <TooltipTrigger asChild>
                <Button
                  type="button"
                  variant="outline"
                  size="icon"
                  onClick={handleAddPeriod}
                  aria-label="Add moving average"
                  className="p-0"
                >
                  <Icon icon={icons.plus} />
                </Button>
              </TooltipTrigger>
              <TooltipContent>Add moving average</TooltipContent>
            </Tooltip>
          </div>
        ) : null}

        {periodEntries.map(([key]) => (
          <MovingAveragePeriod
            key={key}
            control={form.control}
            name={`periods.${key}`}
            isReadOnly={isReadOnly}
            isHorizontal={isHorizontal}
            onRemove={() => {
              const current = form.getValues("periods") ?? {};
              const nextPeriods = { ...current };
              delete nextPeriods[key];
              form.setValue("periods", nextPeriods);
            }}
          />
        ))}
      </div>
    );
  };

  return (
    <form
      onSubmit={
        onSubmit
          ? (event) => {
              void form.handleSubmit(handleSubmit)(event);
            }
          : undefined
      }
      className={cn(showSubmitButton && "space-y-6")}
    >
      <div className={cn("flex flex-col", isCompact ? "gap-1.5" : "gap-2")}>
        {showParametersOnly ? (
          <FieldSet
            className={cn(
              "space-y-4 rounded-2xl shadow-sm lg:col-span-4",
              isCompact ? "p-3" : "p-4"
            )}
          >
            <FieldGroup
              className={cn("flex flex-col", isCompact ? "gap-3" : "gap-4")}
            >
              {renderParameterFields(true)}
            </FieldGroup>
          </FieldSet>
        ) : (
          <div className="flex flex-col gap-4">
            <FieldSet
              className={cn(
                "w-full min-w-0 space-y-4 rounded-2xl shadow-sm md:w-1/2 lg:w-1/4",
                isCompact ? "p-3" : "p-4"
              )}
            >
              <FieldGroup>
                <Collapsible
                  open={strategyOpen}
                  onOpenChange={handleStrategyOpenChange}
                  className="group/collapsible"
                >
                  <div className="flex items-center gap-1">
                    <div>
                      <p className="text-md">Strategy</p>
                      <p className="text-sm text-muted-foreground">
                        Core strategy metadata.
                      </p>
                    </div>

                    <CollapsibleTrigger asChild>
                      <Button
                        variant="outline"
                        size="icon"
                        aria-label="Toggle details"
                        className="ml-auto p-0"
                      >
                        <Icon
                          icon={
                            strategyOpen ? icons.minimize2 : icons.maximize2
                          }
                        />
                      </Button>
                    </CollapsibleTrigger>
                  </div>

                  <CollapsibleContent className="flex flex-col gap-4 pt-4">
                    <IntegerField
                      control={form.control}
                      name="strategyId"
                      label="Strategy ID"
                      isReadOnly={isReadOnly}
                    />
                    <TextField
                      control={form.control}
                      name="name"
                      label="Name"
                      isReadOnly={isReadOnly}
                    />
                    <TextField
                      control={form.control}
                      name="symbol"
                      label="Symbol"
                      isReadOnly={isReadOnly}
                    />
                    <TextAreaField
                      control={form.control}
                      name="description"
                      label="Description"
                      isReadOnly={isReadOnly}
                    />

                    <EnumSelectField
                      control={form.control}
                      name="strategyProcessorType"
                      label="Strategy processor type"
                      options={strategyProcessorTypeOptions}
                      isReadOnly={isReadOnly}
                    />

                    <EnumSelectField
                      control={form.control}
                      name="strategyEngineType"
                      label="Strategy engine type"
                      options={strategyEngineTypeOptions}
                      isReadOnly={isReadOnly}
                    />

                    <EnumSelectField
                      control={form.control}
                      name="exchange"
                      label="Exchange"
                      options={exchangeOptions}
                      isReadOnly={isReadOnly}
                    />
                  </CollapsibleContent>
                </Collapsible>
              </FieldGroup>
            </FieldSet>

            <FieldSet
              className={cn(
                "w-full min-w-0 space-y-4 rounded-2xl shadow-sm md:w-1/2 lg:w-1/4",
                isCompact ? "p-3" : "p-4"
              )}
            >
              <FieldGroup>
                <Collapsible
                  open={parametersOpen}
                  onOpenChange={handleParametersOpenChange}
                  className="group/collapsible"
                >
                  <div className="flex items-center gap-1">
                    <div>
                      <p className="text-md">Parameters</p>
                      <p className="text-sm text-muted-foreground">
                        Strategy logic inputs.
                      </p>
                    </div>

                    <CollapsibleTrigger asChild>
                      <Button
                        variant="outline"
                        size="icon"
                        aria-label="Toggle details"
                        className="ml-auto p-0"
                      >
                        <Icon
                          icon={
                            parametersOpen ? icons.minimize2 : icons.maximize2
                          }
                        />
                      </Button>
                    </CollapsibleTrigger>
                  </div>

                  <CollapsibleContent className="flex flex-col gap-4 pt-4">
                    {renderParameterFields()}
                  </CollapsibleContent>
                </Collapsible>
              </FieldGroup>
            </FieldSet>

            <FieldSet
              className={cn(
                "w-full min-w-0 space-y-4 rounded-2xl shadow-sm md:w-1/2 lg:w-1/4",
                isCompact ? "p-3" : "p-4"
              )}
            >
              <FieldGroup>
                <Collapsible
                  open={subscriptionOpen}
                  onOpenChange={handleSubscriptionOpenChange}
                  className="group/collapsible"
                >
                  <div className="flex items-center gap-1">
                    <div>
                      <p className="text-md">Subscription and caching</p>
                      <p className="text-sm text-muted-foreground">
                        Kline, trade, cache, and processor settings.
                      </p>
                    </div>

                    <CollapsibleTrigger asChild>
                      <Button
                        variant="outline"
                        size="icon"
                        aria-label="Toggle details"
                        className="ml-auto p-0"
                      >
                        <Icon
                          icon={
                            subscriptionOpen ? icons.minimize2 : icons.maximize2
                          }
                        />
                      </Button>
                    </CollapsibleTrigger>
                  </div>

                  <CollapsibleContent className="flex flex-col gap-4 pt-4">
                    <EnumSelectField
                      control={form.control}
                      name="klineInterval"
                      label="Kline interval"
                      options={klineIntervalOptions}
                      isReadOnly={isReadOnly}
                    />

                    <IntegerField
                      control={form.control}
                      name="klineSeedSize"
                      label="Kline seed size"
                      isReadOnly={isReadOnly}
                    />

                    <IntegerField
                      control={form.control}
                      name="klineSeedLimit"
                      label="Kline seed limit"
                      isReadOnly={isReadOnly}
                    />

                    <NullableIntegerField
                      control={form.control}
                      name="orderBookLimit"
                      label="Order book limit"
                      description="Leave empty to submit null."
                      isReadOnly={isReadOnly}
                    />

                    <IntegerField
                      control={form.control}
                      name="maxOrderBookAgeSeconds"
                      label="Max order book age seconds"
                      isReadOnly={isReadOnly}
                    />

                    <IntegerField
                      control={form.control}
                      name="maxAccountAgeSeconds"
                      label="Max account age seconds"
                      isReadOnly={isReadOnly}
                    />

                    <IntegerField
                      control={form.control}
                      name="cacheMaxKlinesPerSeries"
                      label="Cache max klines per series"
                      isReadOnly={isReadOnly}
                    />

                    <IntegerField
                      control={form.control}
                      name="cacheMaxTradesPerSymbol"
                      label="Cache max trades per symbol"
                      isReadOnly={isReadOnly}
                    />

                    <IntegerField
                      control={form.control}
                      name="cacheMaxIndicatorsPerSeries"
                      label="Cache max indicators per series"
                      isReadOnly={isReadOnly}
                    />

                    <IntegerField
                      control={form.control}
                      name="cacheMaxSignalsPerSeries"
                      label="Cache max signals per series"
                      isReadOnly={isReadOnly}
                    />

                    <IntegerField
                      control={form.control}
                      name="strategyProcessorMaxTradesPerPass"
                      label="Strategy processor max trades per pass"
                      isReadOnly={isReadOnly}
                    />

                    <IntegerField
                      control={form.control}
                      name="subscriptionChannelKlineCapacity"
                      label="Subscription channel kline capacity"
                      isReadOnly={isReadOnly}
                    />

                    <EnumSelectField
                      control={form.control}
                      name="subscriptionChannelKlineFullMode"
                      label="Subscription channel kline full mode"
                      options={boundedChannelFullModeOptions}
                      isReadOnly={isReadOnly}
                    />

                    <IntegerField
                      control={form.control}
                      name="subscriptionChannelTradeCapacity"
                      label="Subscription channel trade capacity"
                      isReadOnly={isReadOnly}
                    />

                    <EnumSelectField
                      control={form.control}
                      name="subscriptionChannelTradeFullMode"
                      label="Subscription channel trade full mode"
                      options={boundedChannelFullModeOptions}
                      isReadOnly={isReadOnly}
                    />

                    <IntegerField
                      control={form.control}
                      name="marketEventDispatcherCapacity"
                      label="Market event dispatcher capacity"
                      isReadOnly={isReadOnly}
                    />

                    <EnumSelectField
                      control={form.control}
                      name="marketEventDispatcherFullMode"
                      label="Market event dispatcher full mode"
                      options={boundedChannelFullModeOptions}
                      isReadOnly={isReadOnly}
                    />
                  </CollapsibleContent>
                </Collapsible>
              </FieldGroup>
            </FieldSet>

            <FieldSet
              className={cn(
                "w-full min-w-0 space-y-4 rounded-2xl shadow-sm md:w-1/2 lg:w-1/4",
                isCompact ? "p-3" : "p-4"
              )}
            >
              <FieldGroup>
                <Collapsible
                  open={broadcastOpen}
                  onOpenChange={handleBroadcastOpenChange}
                  className="group/collapsible"
                >
                  <div className="flex items-center gap-1">
                    <div>
                      <p className="text-md">Broadcast</p>
                      <p className="text-sm text-muted-foreground">
                        Broadcast channel behavior.
                      </p>
                    </div>

                    <CollapsibleTrigger asChild>
                      <Button
                        variant="outline"
                        size="icon"
                        aria-label="Toggle details"
                        className="ml-auto p-0"
                      >
                        <Icon
                          icon={
                            broadcastOpen ? icons.minimize2 : icons.maximize2
                          }
                        />
                      </Button>
                    </CollapsibleTrigger>
                  </div>

                  <CollapsibleContent className="flex flex-col gap-4 pt-4">
                    <IntegerField
                      control={form.control}
                      name="klineBroadcastCapacity"
                      label="Kline broadcast capacity"
                      isReadOnly={isReadOnly}
                    />

                    <EnumSelectField
                      control={form.control}
                      name="klineBroadcastFullMode"
                      label="Kline broadcast full mode"
                      options={boundedChannelFullModeOptions}
                      isReadOnly={isReadOnly}
                    />

                    <IntegerField
                      control={form.control}
                      name="tradeBroadcastCapacity"
                      label="Trade broadcast capacity"
                      isReadOnly={isReadOnly}
                    />

                    <EnumSelectField
                      control={form.control}
                      name="tradeBroadcastFullMode"
                      label="Trade broadcast full mode"
                      options={boundedChannelFullModeOptions}
                      isReadOnly={isReadOnly}
                    />

                    <IntegerField
                      control={form.control}
                      name="indicatorsBroadcastCapacity"
                      label="Indicators broadcast capacity"
                      isReadOnly={isReadOnly}
                    />

                    <EnumSelectField
                      control={form.control}
                      name="indicatorsBroadcastFullMode"
                      label="Indicators broadcast full mode"
                      options={boundedChannelFullModeOptions}
                      isReadOnly={isReadOnly}
                    />

                    <IntegerField
                      control={form.control}
                      name="signalBroadcastCapacity"
                      label="Signal broadcast capacity"
                      isReadOnly={isReadOnly}
                    />

                    <EnumSelectField
                      control={form.control}
                      name="signalBroadcastFullMode"
                      label="Signal broadcast full mode"
                      options={boundedChannelFullModeOptions}
                      isReadOnly={isReadOnly}
                    />
                  </CollapsibleContent>
                </Collapsible>
              </FieldGroup>
            </FieldSet>
          </div>
        )}
      </div>

      {showSubmitButton ? (
        <div className="flex justify-end">
          <Button
            type="submit"
            disabled={isReadOnly || !onSubmit || form.formState.isSubmitting}
          >
            {form.formState.isSubmitting ? "Saving..." : submitLabel}
          </Button>
        </div>
      ) : null}
    </form>
  );
}

export default StrategyForm;
