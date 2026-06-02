"use client";

import * as React from "react";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { Controller, useForm, type Control } from "react-hook-form";
import { Button } from "@/components/ui/button";
import {
  Field,
  FieldDescription,
  FieldError,
  FieldGroup,
  FieldLabel,
  FieldSet,
} from "@/components/ui/field";
import { Input } from "@/components/ui/input";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { Textarea } from "@/components/ui/textarea";
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
import {
  MovingAverageSmoothingType,
  MovingAverageSmoothingTypeLabels,
} from "@/features/api/schema/moving-average-smothing-type";
import {
  StrategyProcessorType,
  StrategyProcessorTypeLabels,
} from "@/features/api/schema/strategy-processor-type";
import {
  StrategySchema,
  type Strategy,
} from "@/features/api/schema/strategy-schema";

import { enumToOptions, type EnumOption } from "@/lib/enum-helper";
import { cn } from "@/lib/utils";

type StrategyFormValues = z.input<typeof StrategySchema>;

type StrategyIntegerFieldName = Extract<
  keyof Strategy,
  | "strategyId"
  | "klineSeedSize"
  | "klineSeedLimit"
  | "maxOrderBookAgeSeconds"
  | "maxAccountAgeSeconds"
  | "cacheMaxKlinesPerSeries"
  | "cacheMaxTradesPerSymbol"
  | "cacheMaxIndicatorsPerSeries"
  | "cacheMaxSignalsPerSeries"
  | "strategyProcessorMaxTradesPerPass"
  | "subscriptionChannelKlineCapacity"
  | "subscriptionChannelTradeCapacity"
  | "klineBroadcastCapacity"
  | "tradeBroadcastCapacity"
  | "indicatorsBroadcastCapacity"
  | "signalBroadcastCapacity"
>;

type StrategyEnumFieldName = Extract<
  keyof Strategy,
  | "strategyProcessorType"
  | "strategyEngineType"
  | "smoothingType"
  | "exchange"
  | "klineInterval"
  | "subscriptionChannelTradeFullMode"
  | "subscriptionChannelKlineFullMode"
  | "klineBroadcastFullMode"
  | "tradeBroadcastFullMode"
  | "indicatorsBroadcastFullMode"
  | "signalBroadcastFullMode"
>;

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
const movingAverageSmoothingTypeOptions = enumToOptions(
  MovingAverageSmoothingType,
  MovingAverageSmoothingTypeLabels
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
  smoothingType: MovingAverageSmoothingType.Sma,
  periods: {
    "9 SMA": 9,
    "21 SMA": 21,
    "50 SMA": 50,
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
  subscriptionChannelTradeCapacity: 10000,
  subscriptionChannelTradeFullMode: BoundedChannelFullMode.DropOldest,
  subscriptionChannelKlineFullMode: BoundedChannelFullMode.DropOldest,
  klineBroadcastCapacity: 500,
  tradeBroadcastCapacity: 10000,
  klineBroadcastFullMode: BoundedChannelFullMode.DropOldest,
  tradeBroadcastFullMode: BoundedChannelFullMode.DropOldest,
  indicatorsBroadcastCapacity: 5000,
  signalBroadcastCapacity: 5000,
  indicatorsBroadcastFullMode: BoundedChannelFullMode.DropOldest,
  signalBroadcastFullMode: BoundedChannelFullMode.DropOldest,
};

function nullableTextToInputValue(value: string | null | undefined): string {
  return value ?? "";
}

function inputTextToNullable(value: string): string | null {
  const trimmed = value.trim();
  return trimmed.length === 0 ? null : trimmed;
}

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

function numberToSelectValue(value: number): string {
  return String(value);
}

function selectValueToNumber<TValue extends number>(value: string): TValue {
  return Number.parseInt(value, 10) as TValue;
}

function nullableNumberInputValue(value: number | null | undefined): string {
  return value === null || value === undefined ? "" : String(value);
}

function parseIntegerInput(value: string): number | undefined {
  if (value.trim() === "") return undefined;

  const parsed = Number.parseInt(value, 10);
  return Number.isNaN(parsed) ? undefined : parsed;
}

function parseNullableIntegerInput(value: string): number | null | undefined {
  if (value.trim() === "") return null;

  const parsed = Number.parseInt(value, 10);
  return Number.isNaN(parsed) ? undefined : parsed;
}

function TextField({
  control,
  name,
  label,
  isReadOnly,
}: {
  control: Control<StrategyFormValues>;
  name: Extract<keyof StrategyFormValues, "name" | "symbol">;
  label: string;
  isReadOnly: boolean;
}) {
  return (
    <Controller
      control={control}
      name={name}
      render={({ field, fieldState }) => (
        <Field data-invalid={fieldState.invalid}>
          <FieldLabel htmlFor={field.name}>{label}</FieldLabel>
          <Input
            id={field.name}
            name={field.name}
            ref={field.ref}
            value={nullableTextToInputValue(field.value)}
            onBlur={field.onBlur}
            onChange={(event) => field.onChange(event.target.value)}
            disabled={isReadOnly}
            aria-invalid={fieldState.invalid}
          />
          {fieldState.invalid ? (
            <FieldError errors={[fieldState.error]} />
          ) : null}
        </Field>
      )}
    />
  );
}

function TextAreaField({
  control,
  name,
  label,
  isReadOnly,
}: {
  control: Control<StrategyFormValues>;
  name: Extract<keyof StrategyFormValues, "description">;
  label: string;
  isReadOnly: boolean;
}) {
  return (
    <Controller
      control={control}
      name={name}
      render={({ field, fieldState }) => (
        <Field data-invalid={fieldState.invalid}>
          <FieldLabel htmlFor={field.name}>{label}</FieldLabel>
          <Textarea
            id={field.name}
            name={field.name}
            ref={field.ref}
            value={nullableTextToInputValue(field.value)}
            onBlur={field.onBlur}
            onChange={(event) => field.onChange(event.target.value)}
            disabled={isReadOnly}
            aria-invalid={fieldState.invalid}
          />
          {fieldState.invalid ? (
            <FieldError errors={[fieldState.error]} />
          ) : null}
        </Field>
      )}
    />
  );
}

function IntegerField({
  control,
  name,
  label,
  isReadOnly,
  isHorizontal = false,
}: {
  control: Control<StrategyFormValues>;
  name: StrategyIntegerFieldName;
  label: string;
  isReadOnly: boolean;
  isHorizontal?: boolean;
}) {
  return (
    <Controller
      control={control}
      name={name}
      render={({ field, fieldState }) => (
        <Field
          orientation={isHorizontal ? "horizontal" : "vertical"}
          data-invalid={fieldState.invalid}
          className={
            isHorizontal
              ? "gap-1.5 [&>[data-slot=field-label]]:w-28 [&>[data-slot=field-label]]:flex-none [&>[data-slot=field-label]]:whitespace-nowrap"
              : undefined
          }
        >
          <FieldLabel htmlFor={field.name}>{label}</FieldLabel>
          <Input
            id={field.name}
            name={field.name}
            ref={field.ref}
            type="number"
            inputMode="numeric"
            step={1}
            value={nullableNumberInputValue(field.value)}
            onBlur={field.onBlur}
            onChange={(event) =>
              field.onChange(parseIntegerInput(event.target.value))
            }
            disabled={isReadOnly}
            aria-invalid={fieldState.invalid}
          />
          {fieldState.invalid ? (
            <FieldError errors={[fieldState.error]} />
          ) : null}
        </Field>
      )}
    />
  );
}

function NullableIntegerField({
  control,
  name,
  label,
  description,
  isReadOnly,
}: {
  control: Control<StrategyFormValues>;
  name: Extract<keyof StrategyFormValues, "orderBookLimit">;
  label: string;
  description?: string;
  isReadOnly: boolean;
}) {
  return (
    <Controller
      control={control}
      name={name}
      render={({ field, fieldState }) => (
        <Field data-invalid={fieldState.invalid}>
          <FieldLabel htmlFor={field.name}>{label}</FieldLabel>
          <Input
            id={field.name}
            name={field.name}
            ref={field.ref}
            type="number"
            inputMode="numeric"
            step={1}
            value={nullableNumberInputValue(field.value)}
            onBlur={field.onBlur}
            onChange={(event) =>
              field.onChange(parseNullableIntegerInput(event.target.value))
            }
            disabled={isReadOnly}
            aria-invalid={fieldState.invalid}
          />
          {description ? (
            <FieldDescription>{description}</FieldDescription>
          ) : null}
          {fieldState.invalid ? (
            <FieldError errors={[fieldState.error]} />
          ) : null}
        </Field>
      )}
    />
  );
}

function EnumSelectField<TName extends StrategyEnumFieldName>({
  control,
  name,
  label,
  options,
  isReadOnly,
  isHorizontal = false,
}: {
  control: Control<StrategyFormValues>;
  name: TName;
  label: string;
  options: EnumOption<Extract<Strategy[TName], number>>[];
  isReadOnly: boolean;
  isHorizontal?: boolean;
}) {
  return (
    <Controller
      control={control}
      name={name}
      render={({ field, fieldState }) => (
        <Field
          orientation={isHorizontal ? "horizontal" : "vertical"}
          data-invalid={fieldState.invalid}
          className={
            isHorizontal
              ? "gap-1.5 [&>[data-slot=field-label]]:w-28 [&>[data-slot=field-label]]:flex-none [&>[data-slot=field-label]]:whitespace-nowrap"
              : undefined
          }
        >
          <FieldLabel>{label}</FieldLabel>
          <Select
            value={numberToSelectValue(field.value as number)}
            onValueChange={(value) =>
              field.onChange(
                selectValueToNumber<Extract<Strategy[TName], number>>(value)
              )
            }
            disabled={isReadOnly}
          >
            <SelectTrigger
              aria-invalid={fieldState.invalid}
              className={isHorizontal ? "flex-1" : undefined}
            >
              <SelectValue placeholder={`Select ${label.toLowerCase()}`} />
            </SelectTrigger>
            <SelectContent>
              {options.map((option) => (
                <SelectItem
                  key={option.value}
                  value={numberToSelectValue(option.value)}
                >
                  {option.label}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
          {fieldState.invalid ? (
            <FieldError errors={[fieldState.error]} />
          ) : null}
        </Field>
      )}
    />
  );
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

  const renderParameterFields = (isHorizontal = false) => (
    <>
      <EnumSelectField
        control={form.control}
        name="smoothingType"
        label="Smoothing type"
        options={movingAverageSmoothingTypeOptions}
        isReadOnly={isReadOnly}
        isHorizontal={isHorizontal}
      />
    </>
  );

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
      <div
        className={cn(
          "grid grid-cols-1 lg:grid-cols-4",
          isCompact ? "gap-1.5" : "gap-2"
        )}
      >
        {showParametersOnly ? (
          <FieldSet
            className={cn(
              "space-y-4 rounded-2xl shadow-sm lg:col-span-4",
              isCompact ? "p-3" : "p-4"
            )}
          >
            <FieldGroup
              className={cn(
                "grid grid-cols-1 lg:grid-cols-3",
                isCompact ? "gap-3" : "gap-4"
              )}
            >
              {renderParameterFields(true)}
            </FieldGroup>
          </FieldSet>
        ) : (
          <>
            <FieldSet
              className={cn(
                "space-y-4 rounded-2xl shadow-sm",
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
                        Core strategy metadata and runtime selections.
                      </p>
                    </div>

                    <CollapsibleTrigger asChild>
                      <Button
                        variant="outline"
                        size="icon"
                        aria-label="Toggle details"
                        className="p-0"
                      >
                        <Icon icon={strategyOpen ? icons.minus : icons.plus} />
                      </Button>
                    </CollapsibleTrigger>
                  </div>

                  <CollapsibleContent className="space-y-6">
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
                "space-y-4 rounded-2xl shadow-sm",
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
                        className="p-0"
                      >
                        <Icon
                          icon={parametersOpen ? icons.minus : icons.plus}
                        />
                      </Button>
                    </CollapsibleTrigger>
                  </div>

                  <CollapsibleContent className="space-y-6">
                    {renderParameterFields()}
                  </CollapsibleContent>
                </Collapsible>
              </FieldGroup>
            </FieldSet>

            <FieldSet
              className={cn(
                "space-y-4 rounded-2xl shadow-sm",
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
                        Kline, trade, cache, and processor buffer settings.
                      </p>
                    </div>

                    <CollapsibleTrigger asChild>
                      <Button
                        variant="outline"
                        size="icon"
                        aria-label="Toggle details"
                        className="p-0"
                      >
                        <Icon
                          icon={subscriptionOpen ? icons.minus : icons.plus}
                        />
                      </Button>
                    </CollapsibleTrigger>
                  </div>

                  <CollapsibleContent className="space-y-6">
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
                  </CollapsibleContent>
                </Collapsible>
              </FieldGroup>
            </FieldSet>

            <FieldSet
              className={cn(
                "space-y-4 rounded-2xl shadow-sm",
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
                        Broadcast channel capacities and overflow behavior.
                      </p>
                    </div>

                    <CollapsibleTrigger asChild>
                      <Button
                        variant="outline"
                        size="icon"
                        aria-label="Toggle details"
                        className="p-0"
                      >
                        <Icon icon={broadcastOpen ? icons.minus : icons.plus} />
                      </Button>
                    </CollapsibleTrigger>
                  </div>

                  <CollapsibleContent className="space-y-6">
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
          </>
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
