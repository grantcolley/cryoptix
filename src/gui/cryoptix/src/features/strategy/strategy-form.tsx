"use client";

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
  BoundedChannelFullMode,
  BoundedChannelFullModeLabels,
} from "./bounded-channel-full-mode";

import { Exchange, ExchangeLabels } from "./exchange";
import { KlineInterval, KlineIntervalLabels } from "./kline-interval";
import {
  StrategyEngineType,
  StrategyEngineTypeLabels,
} from "./strategy-engine-type";
import {
  StrategyProcessorType,
  StrategyProcessorTypeLabels,
} from "./strategy-processor-type";
import { StrategySchema, type Strategy } from "./strategy-schema";

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
  onSubmit: (strategy: Strategy) => void | Promise<void>;
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
  name: null,
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
  onSubmit,
}: StrategyFormProps) {
  const form = useForm<StrategyFormValues, unknown, Strategy>({
    resolver: zodResolver(StrategySchema),
    defaultValues: {
      ...fallbackDefaultValues,
      ...defaultValues,
    },
  });

  async function handleSubmit(values: Strategy) {
    const normalizedValues = StrategySchema.parse({
      ...values,
      name: inputTextToNullable(nullableTextToInputValue(values.name)),
      description: inputTextToNullable(
        nullableTextToInputValue(values.description)
      ),
      symbol: inputTextToNullable(nullableTextToInputValue(values.symbol)),
    });

    await onSubmit(normalizedValues);
  }

  return (
    <form
      onSubmit={(event) => {
        void form.handleSubmit(handleSubmit)(event);
      }}
      className="space-y-6"
    >
      <div className="grid grid-cols-1 gap-4 lg:grid-cols-3">
        <FieldSet className="space-y-4 rounded-2xl p-4 shadow-sm">
          <FieldGroup>
            <div>
              <h2 className="text-lg font-semibold">Strategy</h2>
              <p className="text-sm text-muted-foreground">
                Core strategy metadata and runtime selections.
              </p>
            </div>

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
          </FieldGroup>
        </FieldSet>

        <FieldSet className="space-y-4 rounded-2xl p-4 shadow-sm">
          <FieldGroup>
            <div>
              <h2 className="text-lg font-semibold">
                Subscription and caching
              </h2>
              <p className="text-sm text-muted-foreground">
                Kline, trade, cache, and processor buffer settings.
              </p>
            </div>

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
          </FieldGroup>
        </FieldSet>

        <FieldSet className="space-y-4 rounded-2xl p-4 shadow-sm">
          <FieldGroup>
            <div>
              <h2 className="text-lg font-semibold">Broadcast</h2>
              <p className="text-sm text-muted-foreground">
                Broadcast channel capacities and overflow behavior.
              </p>
            </div>

            <IntegerField
              name="klineBroadcastCapacity"
              label="Kline broadcast capacity"
            />

            <IntegerField
              name="tradeBroadcastCapacity"
              label="Trade broadcast capacity"
            />

            <EnumSelectField
              name="klineBroadcastFullMode"
              label="Kline broadcast full mode"
              options={boundedChannelFullModeOptions}
            />

            <EnumSelectField
              name="tradeBroadcastFullMode"
              label="Trade broadcast full mode"
              options={boundedChannelFullModeOptions}
            />
          </FieldGroup>
        </FieldSet>
      </div>

      <div className="flex justify-end">
        <Button type="submit" disabled={form.formState.isSubmitting}>
          {form.formState.isSubmitting ? "Saving..." : submitLabel}
        </Button>
      </div>
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
