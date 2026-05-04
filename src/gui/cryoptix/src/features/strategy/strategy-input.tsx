import {
  type Control,
  Controller,
  type FieldPathByValue,
  useForm,
} from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";

import { type Strategy, StrategySchema } from "./strategy-schema";
import { StrategyProcessorType } from "./strategy-processor-type";
import { StrategyEngineType } from "./strategy-engine-type";
import { Exchange } from "./exchange";
import { KlineInterval } from "./kline-interval";
import { BoundedChannelFullMode } from "./bounded-channel-full-mode";

import { Button } from "@/components/ui/button";
import { Checkbox } from "@/components/ui/checkbox";
import { Field, FieldError, FieldLabel } from "@/components/ui/field";
import { Input } from "@/components/ui/input";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";

type NumericStrategyFieldName = FieldPathByValue<
  Strategy,
  number | null | undefined
>;

type StringStrategyFieldName = FieldPathByValue<
  Strategy,
  string | null | undefined
>;

type BooleanStrategyFieldName = FieldPathByValue<Strategy, boolean>;

type SelectOption<T extends number> = {
  label: string;
  value: T;
};

type StrategyFormProps = {
  defaultValues: Strategy;
  onSubmit: (values: Strategy) => void;
};

const strategyProcessorTypeOptions = [
  { label: "None", value: StrategyProcessorType.None },
  { label: "Trading Flow", value: StrategyProcessorType.TradingFlow },
] satisfies SelectOption<Strategy["strategyProcessorType"]>[];

const strategyEngineTypeOptions = [
  { label: "None", value: StrategyEngineType.None },
  { label: "Moving Average", value: StrategyEngineType.MovingAverage },
] satisfies SelectOption<Strategy["strategyEngineType"]>[];

const exchangeOptions = [
  { label: "None", value: Exchange.None },
  { label: "Binance", value: Exchange.Binance },
] satisfies SelectOption<Strategy["exchange"]>[];

const klineIntervalOptions = [
  { label: "Unknown", value: KlineInterval.Unknown },
  { label: "Minute", value: KlineInterval.Minute },
  { label: "3 Minutes", value: KlineInterval.Minutes3 },
  { label: "5 Minutes", value: KlineInterval.Minutes5 },
  { label: "15 Minutes", value: KlineInterval.Minutes15 },
  { label: "30 Minutes", value: KlineInterval.Minutes30 },
  { label: "Hour", value: KlineInterval.Hour },
  { label: "2 Hours", value: KlineInterval.Hours2 },
  { label: "4 Hours", value: KlineInterval.Hours4 },
  { label: "6 Hours", value: KlineInterval.Hours6 },
  { label: "8 Hours", value: KlineInterval.Hours8 },
  { label: "12 Hours", value: KlineInterval.Hours12 },
  { label: "Day", value: KlineInterval.Day },
  { label: "3 Days", value: KlineInterval.Days3 },
  { label: "Week", value: KlineInterval.Week },
  { label: "Month", value: KlineInterval.Month },
] satisfies SelectOption<Strategy["klineInterval"]>[];

const boundedChannelFullModeOptions = [
  { label: "Wait", value: BoundedChannelFullMode.Wait },
  { label: "Drop Newest", value: BoundedChannelFullMode.DropNewest },
  { label: "Drop Oldest", value: BoundedChannelFullMode.DropOldest },
  { label: "Drop Write", value: BoundedChannelFullMode.DropWrite },
] satisfies SelectOption<Strategy["klineBroadcastFullMode"]>[];

type NumberFieldProps = {
  control: Control<Strategy>;
  name: NumericStrategyFieldName;
  label: string;
  nullable?: boolean;
};

function NumberField({
  control,
  name,
  label,
  nullable = false,
}: NumberFieldProps) {
  return (
    <Controller
      control={control}
      name={name}
      render={({ field, fieldState }) => {
        const value = field.value;

        return (
          <Field>
            <FieldLabel>{label}</FieldLabel>
            <Input
              type="number"
              value={value ?? ""}
              onChange={(event) => {
                const rawValue = event.target.value;

                if (rawValue === "") {
                  field.onChange(nullable ? null : undefined);
                  return;
                }

                field.onChange(Number.parseInt(rawValue, 10));
              }}
            />
            {fieldState.error ? (
              <FieldError>{fieldState.error.message}</FieldError>
            ) : null}
          </Field>
        );
      }}
    />
  );
}

type TextFieldProps = {
  control: Control<Strategy>;
  name: StringStrategyFieldName;
  label: string;
};

function TextField({ control, name, label }: TextFieldProps) {
  return (
    <Controller
      control={control}
      name={name}
      render={({ field, fieldState }) => (
        <Field>
          <FieldLabel>{label}</FieldLabel>
          <Input
            value={field.value ?? ""}
            onChange={(event) => {
              const rawValue = event.target.value;
              field.onChange(rawValue === "" ? null : rawValue);
            }}
          />
          {fieldState.error ? (
            <FieldError>{fieldState.error.message}</FieldError>
          ) : null}
        </Field>
      )}
    />
  );
}

type SelectFieldProps<TName extends NumericStrategyFieldName> = {
  control: Control<Strategy>;
  name: TName;
  label: string;
  options: SelectOption<Extract<Strategy[TName], number>>[];
};

function SelectField<TName extends NumericStrategyFieldName>({
  control,
  name,
  label,
  options,
}: SelectFieldProps<TName>) {
  return (
    <Controller
      control={control}
      name={name}
      render={({ field, fieldState }) => (
        <Field>
          <FieldLabel>{label}</FieldLabel>

          <Select
            value={field.value == null ? "" : String(field.value)}
            onValueChange={(selectedValue) => {
              const selected = options.find(
                (option) => String(option.value) === selectedValue
              );

              if (selected) {
                field.onChange(selected.value);
              }
            }}
          >
            <SelectTrigger>
              <SelectValue placeholder={`Select ${label}`} />
            </SelectTrigger>

            <SelectContent>
              {options.map((option) => (
                <SelectItem key={option.value} value={String(option.value)}>
                  {option.label}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>

          {fieldState.error ? (
            <FieldError>{fieldState.error.message}</FieldError>
          ) : null}
        </Field>
      )}
    />
  );
}

type CheckboxFieldProps = {
  control: Control<Strategy>;
  name: BooleanStrategyFieldName;
  label: string;
};

function CheckboxField({ control, name, label }: CheckboxFieldProps) {
  return (
    <Controller
      control={control}
      name={name}
      render={({ field, fieldState }) => (
        <Field className="flex flex-row items-center gap-2">
          <Checkbox
            checked={field.value}
            onCheckedChange={(checked) => {
              field.onChange(checked === true);
            }}
          />

          <FieldLabel>{label}</FieldLabel>

          {fieldState.error ? (
            <FieldError>{fieldState.error.message}</FieldError>
          ) : null}
        </Field>
      )}
    />
  );
}

export function StrategyForm({ defaultValues, onSubmit }: StrategyFormProps) {
  const form = useForm<Strategy>({
    resolver: zodResolver(StrategySchema),
    defaultValues,
  });

  return (
    <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-4">
      <div className="grid grid-cols-3 gap-4">
        <div className="space-y-4 rounded-xl border p-4">
          <h3 className="font-semibold">Strategy fields</h3>

          <NumberField
            control={form.control}
            name="strategyId"
            label="Strategy ID"
          />
          <TextField control={form.control} name="name" label="Name" />
          <TextField
            control={form.control}
            name="description"
            label="Description"
          />
          <TextField control={form.control} name="symbol" label="Symbol" />

          <SelectField
            control={form.control}
            name="strategyProcessorType"
            label="Processor Type"
            options={strategyProcessorTypeOptions}
          />

          <SelectField
            control={form.control}
            name="strategyEngineType"
            label="Engine Type"
            options={strategyEngineTypeOptions}
          />

          <SelectField
            control={form.control}
            name="exchange"
            label="Exchange"
            options={exchangeOptions}
          />
        </div>

        <div className="space-y-4 rounded-xl border p-4">
          <h3 className="font-semibold">Subscription and caching fields</h3>

          <SelectField
            control={form.control}
            name="klineInterval"
            label="Kline Interval"
            options={klineIntervalOptions}
          />

          <NumberField
            control={form.control}
            name="fastPeriod"
            label="Fast Period"
          />
          <NumberField
            control={form.control}
            name="slowPeriod"
            label="Slow Period"
          />
          <NumberField
            control={form.control}
            name="orderBookLimit"
            label="Order Book Limit"
            nullable
          />
          <NumberField
            control={form.control}
            name="maxOrderBookAgeSeconds"
            label="Max Order Book Age Seconds"
          />
          <NumberField
            control={form.control}
            name="maxAccountAgeSeconds"
            label="Max Account Age Seconds"
          />
          <NumberField
            control={form.control}
            name="cacheMaxKlinesPerSeries"
            label="Cache Max Klines Per Series"
          />
          <NumberField
            control={form.control}
            name="cacheMaxTradesPerSymbol"
            label="Cache Max Trades Per Symbol"
          />
          <NumberField
            control={form.control}
            name="strategyProcessorMaxTradesPerPass"
            label="Processor Max Trades Per Pass"
          />
          <NumberField
            control={form.control}
            name="subscriptionChannelKlineCapacity"
            label="Subscription Kline Capacity"
          />
          <NumberField
            control={form.control}
            name="subscriptionChannelTradeCapacity"
            label="Subscription Trade Capacity"
          />

          <CheckboxField
            control={form.control}
            name="subscriptionChannelDropTradesWhenFull"
            label="Drop trades when full"
          />

          <SelectField
            control={form.control}
            name="subscriptionChannelKlineFullMode"
            label="Subscription Kline Full Mode"
            options={boundedChannelFullModeOptions}
          />
        </div>

        <div className="space-y-4 rounded-xl border p-4">
          <h3 className="font-semibold">Broadcast fields</h3>

          <NumberField
            control={form.control}
            name="klineBroadcastCapacity"
            label="Kline Broadcast Capacity"
          />
          <NumberField
            control={form.control}
            name="tradeBroadcastCapacity"
            label="Trade Broadcast Capacity"
          />

          <SelectField
            control={form.control}
            name="klineBroadcastFullMode"
            label="Kline Broadcast Full Mode"
            options={boundedChannelFullModeOptions}
          />

          <SelectField
            control={form.control}
            name="tradeBroadcastFullMode"
            label="Trade Broadcast Full Mode"
            options={boundedChannelFullModeOptions}
          />
        </div>
      </div>

      <Button type="submit">Save strategy</Button>
    </form>
  );
}
