"use client";

import * as React from "react";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { Controller, useForm } from "react-hook-form";
import { Button } from "@/components/ui/button";
import { Checkbox } from "@/components/ui/checkbox";
import {
  Field,
  FieldContent,
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
  StrategyProcessorType,
  StrategyProcessorTypeLabels,
} from "@/features/api/schema/strategy-processor-type";
import {
  StrategySchema,
  type Strategy,
} from "@/features/api/schema/strategy-schema";

import { enumToOptions, type EnumOption } from "@/lib/enum-helper";

type StrategyFormValues = z.input<typeof StrategySchema>;

type StrategyIntegerFieldName = Extract<
  keyof Strategy,
  | "strategyId"
  | "fastPeriod"
  | "slowPeriod"
  | "maxOrderBookAgeSeconds"
  | "maxAccountAgeSeconds"
  | "cacheMaxKlinesPerSeries"
  | "cacheMaxTradesPerSymbol"
  | "strategyProcessorMaxTradesPerPass"
  | "subscriptionChannelKlineCapacity"
  | "subscriptionChannelTradeCapacity"
  | "klineBroadcastCapacity"
  | "tradeBroadcastCapacity"
>;

type StrategyEnumFieldName = Extract<
  keyof Strategy,
  | "strategyProcessorType"
  | "strategyEngineType"
  | "exchange"
  | "klineInterval"
  | "subscriptionChannelKlineFullMode"
  | "klineBroadcastFullMode"
  | "tradeBroadcastFullMode"
>;

type StrategyFormProps = {
  defaultValues?: Partial<Strategy>;
  submitLabel?: string;
  showSubmitButton?: boolean;
  onSubmit?: (strategy: Strategy) => void | Promise<void>;
  onChange?: (strategy: Strategy) => void;
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
  klineInterval: KlineInterval.Minute,
  fastPeriod: 0,
  slowPeriod: 0,
  orderBookLimit: 20,
  maxOrderBookAgeSeconds: 3,
  maxAccountAgeSeconds: 10,
  cacheMaxKlinesPerSeries: 5000,
  cacheMaxTradesPerSymbol: 10000,
  strategyProcessorMaxTradesPerPass: 256,
  subscriptionChannelKlineCapacity: 500,
  subscriptionChannelTradeCapacity: 10000,
  subscriptionChannelDropTradesWhenFull: true,
  subscriptionChannelKlineFullMode: BoundedChannelFullMode.Wait,
  klineBroadcastCapacity: 500,
  tradeBroadcastCapacity: 10000,
  klineBroadcastFullMode: BoundedChannelFullMode.DropOldest,
  tradeBroadcastFullMode: BoundedChannelFullMode.DropOldest,
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

export function StrategyForm({
  defaultValues,
  submitLabel = "Save strategy",
  showSubmitButton = true,
  onSubmit,
  onChange,
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

  const [isSubscriptionOpen, setIsSubscriptionOpen] = React.useState(false);
  const [isStrategyOpen, setIsStrategyOpen] = React.useState(false);
  const [isBroadcastOpen, setIsBroadcastOpen] = React.useState(false);

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

  return (
    <form
      onSubmit={
        onSubmit
          ? (event) => {
              void form.handleSubmit(handleSubmit)(event);
            }
          : undefined
      }
      className="space-y-6"
    >
      <div className="grid grid-cols-1 gap-4 lg:grid-cols-3">
        <FieldSet className="space-y-4 rounded-2xl p-4 shadow-sm">
          <FieldGroup>
            <Collapsible
              open={isStrategyOpen}
              onOpenChange={setIsStrategyOpen}
              className="group/collapsible"
            >
              <div className="flex items-center gap-1 space-y-6">
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
                    <Icon icon={isStrategyOpen ? icons.minus : icons.plus} />
                  </Button>
                </CollapsibleTrigger>
              </div>

              <CollapsibleContent className="space-y-6">
                <IntegerField name="strategyId" label="Strategy ID" />
                <TextField name="name" label="Name" />
                <TextField name="symbol" label="Symbol" />
                <TextAreaField name="description" label="Description" />

                <EnumSelectField
                  name="strategyProcessorType"
                  label="Strategy processor type"
                  options={strategyProcessorTypeOptions}
                />

                <EnumSelectField
                  name="strategyEngineType"
                  label="Strategy engine type"
                  options={strategyEngineTypeOptions}
                />

                <EnumSelectField
                  name="exchange"
                  label="Exchange"
                  options={exchangeOptions}
                />
              </CollapsibleContent>
            </Collapsible>
          </FieldGroup>
        </FieldSet>

        <FieldSet className="space-y-4 rounded-2xl p-4 shadow-sm">
          <FieldGroup>
            <Collapsible
              open={isSubscriptionOpen}
              onOpenChange={setIsSubscriptionOpen}
              className="group/collapsible"
            >
              <div className="flex items-center gap-1 space-y-6">
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
                      icon={isSubscriptionOpen ? icons.minus : icons.plus}
                    />
                  </Button>
                </CollapsibleTrigger>
              </div>

              <CollapsibleContent className="space-y-6">
                <EnumSelectField
                  name="klineInterval"
                  label="Kline interval"
                  options={klineIntervalOptions}
                />

                <IntegerField name="fastPeriod" label="Fast period" />
                <IntegerField name="slowPeriod" label="Slow period" />

                <NullableIntegerField
                  name="orderBookLimit"
                  label="Order book limit"
                  description="Leave empty to submit null."
                />

                <IntegerField
                  name="maxOrderBookAgeSeconds"
                  label="Max order book age seconds"
                />

                <IntegerField
                  name="maxAccountAgeSeconds"
                  label="Max account age seconds"
                />

                <IntegerField
                  name="cacheMaxKlinesPerSeries"
                  label="Cache max klines per series"
                />

                <IntegerField
                  name="cacheMaxTradesPerSymbol"
                  label="Cache max trades per symbol"
                />

                <IntegerField
                  name="strategyProcessorMaxTradesPerPass"
                  label="Strategy processor max trades per pass"
                />

                <IntegerField
                  name="subscriptionChannelKlineCapacity"
                  label="Subscription channel kline capacity"
                />

                <IntegerField
                  name="subscriptionChannelTradeCapacity"
                  label="Subscription channel trade capacity"
                />

                <BooleanField
                  name="subscriptionChannelDropTradesWhenFull"
                  label="Drop trades when full"
                  description="Drops incoming trades when the subscription channel reaches capacity."
                />

                <EnumSelectField
                  name="subscriptionChannelKlineFullMode"
                  label="Subscription channel kline full mode"
                  options={boundedChannelFullModeOptions}
                />
              </CollapsibleContent>
            </Collapsible>
          </FieldGroup>
        </FieldSet>

        <FieldSet className="space-y-4 rounded-2xl p-4 shadow-sm">
          <FieldGroup>
            <Collapsible
              open={isBroadcastOpen}
              onOpenChange={setIsBroadcastOpen}
              className="group/collapsible"
            >
              <div className="flex items-center gap-1 space-y-6">
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
                    <Icon icon={isBroadcastOpen ? icons.minus : icons.plus} />
                  </Button>
                </CollapsibleTrigger>
              </div>

              <CollapsibleContent className="space-y-6">
                <IntegerField
                  name="klineBroadcastCapacity"
                  label="Kline broadcast capacity"
                />

                <EnumSelectField
                  name="klineBroadcastFullMode"
                  label="Kline broadcast full mode"
                  options={boundedChannelFullModeOptions}
                />

                <IntegerField
                  name="tradeBroadcastCapacity"
                  label="Trade broadcast capacity"
                />

                <EnumSelectField
                  name="tradeBroadcastFullMode"
                  label="Trade broadcast full mode"
                  options={boundedChannelFullModeOptions}
                />
              </CollapsibleContent>
            </Collapsible>
          </FieldGroup>
        </FieldSet>
      </div>

      {showSubmitButton ? (
        <div className="flex justify-end">
          <Button
            type="submit"
            disabled={!onSubmit || form.formState.isSubmitting}
          >
            {form.formState.isSubmitting ? "Saving..." : submitLabel}
          </Button>
        </div>
      ) : null}
    </form>
  );

  function TextField({
    name,
    label,
  }: {
    name: Extract<keyof StrategyFormValues, "name" | "symbol">;
    label: string;
  }) {
    return (
      <Controller
        control={form.control}
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
    name,
    label,
  }: {
    name: Extract<keyof StrategyFormValues, "description">;
    label: string;
  }) {
    return (
      <Controller
        control={form.control}
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
    name,
    label,
  }: {
    name: StrategyIntegerFieldName;
    label: string;
  }) {
    return (
      <Controller
        control={form.control}
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
                field.onChange(parseIntegerInput(event.target.value))
              }
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
    name,
    label,
    description,
  }: {
    name: Extract<keyof StrategyFormValues, "orderBookLimit">;
    label: string;
    description?: string;
  }) {
    return (
      <Controller
        control={form.control}
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

  function BooleanField({
    name,
    label,
    description,
  }: {
    name: Extract<
      keyof StrategyFormValues,
      "subscriptionChannelDropTradesWhenFull"
    >;
    label: string;
    description?: string;
  }) {
    return (
      <Controller
        control={form.control}
        name={name}
        render={({ field, fieldState }) => (
          <Field
            orientation="horizontal"
            data-invalid={fieldState.invalid}
            className="rounded-md border p-3"
          >
            <Checkbox
              id={field.name}
              name={field.name}
              ref={field.ref}
              checked={field.value}
              onBlur={field.onBlur}
              onCheckedChange={(checked) => field.onChange(checked === true)}
              aria-invalid={fieldState.invalid}
            />
            <FieldContent>
              <FieldLabel htmlFor={field.name}>{label}</FieldLabel>
              {description ? (
                <FieldDescription>{description}</FieldDescription>
              ) : null}
              {fieldState.invalid ? (
                <FieldError errors={[fieldState.error]} />
              ) : null}
            </FieldContent>
          </Field>
        )}
      />
    );
  }

  function EnumSelectField<TName extends StrategyEnumFieldName>({
    name,
    label,
    options,
  }: {
    name: TName;
    label: string;
    options: EnumOption<Extract<Strategy[TName], number>>[];
  }) {
    return (
      <Controller
        control={form.control}
        name={name}
        render={({ field, fieldState }) => (
          <Field data-invalid={fieldState.invalid}>
            <FieldLabel>{label}</FieldLabel>
            <Select
              value={numberToSelectValue(field.value as number)}
              onValueChange={(value) =>
                field.onChange(
                  selectValueToNumber<Extract<Strategy[TName], number>>(value)
                )
              }
            >
              <SelectTrigger aria-invalid={fieldState.invalid}>
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
}

export default StrategyForm;
