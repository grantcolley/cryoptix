import {
  Controller,
  type Control,
  type FieldPath,
  type FieldValues,
} from "react-hook-form";
import {
  Field,
  FieldDescription,
  FieldError,
  FieldLabel,
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
import { nullableTextToInputValue } from "@/features/strategy/strategy-form-field-utils";
import type { EnumOption } from "@/lib/enum-helper";

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

type BaseFieldProps<TFieldValues extends FieldValues> = {
  control: Control<TFieldValues>;
  name: FieldPath<TFieldValues>;
  label: string;
  isReadOnly: boolean;
  isHorizontal?: boolean;
};

function horizontalFieldClassName(isHorizontal: boolean) {
  return isHorizontal
    ? "gap-1.5 [&>[data-slot=field-label]]:w-28 [&>[data-slot=field-label]]:flex-none [&>[data-slot=field-label]]:whitespace-nowrap"
    : undefined;
}

export function TextField<TFieldValues extends FieldValues>({
  control,
  name,
  label,
  isReadOnly,
  isHorizontal = false,
}: BaseFieldProps<TFieldValues>) {
  return (
    <Controller
      control={control}
      name={name}
      render={({ field, fieldState }) => (
        <Field
          orientation={isHorizontal ? "horizontal" : "vertical"}
          data-invalid={fieldState.invalid}
          className={horizontalFieldClassName(isHorizontal)}
        >
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

export function TextAreaField<TFieldValues extends FieldValues>({
  control,
  name,
  label,
  isReadOnly,
  isHorizontal = false,
}: BaseFieldProps<TFieldValues>) {
  return (
    <Controller
      control={control}
      name={name}
      render={({ field, fieldState }) => (
        <Field
          orientation={isHorizontal ? "horizontal" : "vertical"}
          data-invalid={fieldState.invalid}
          className={horizontalFieldClassName(isHorizontal)}
        >
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

export function IntegerField<TFieldValues extends FieldValues>({
  control,
  name,
  label,
  isReadOnly,
  isHorizontal = false,
}: BaseFieldProps<TFieldValues>) {
  return (
    <Controller
      control={control}
      name={name}
      render={({ field, fieldState }) => (
        <Field
          orientation={isHorizontal ? "horizontal" : "vertical"}
          data-invalid={fieldState.invalid}
          className={horizontalFieldClassName(isHorizontal)}
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

export function NullableIntegerField<TFieldValues extends FieldValues>({
  control,
  name,
  label,
  description,
  isReadOnly,
  isHorizontal = false,
}: BaseFieldProps<TFieldValues> & {
  description?: string;
}) {
  return (
    <Controller
      control={control}
      name={name}
      render={({ field, fieldState }) => (
        <Field
          orientation={isHorizontal ? "horizontal" : "vertical"}
          data-invalid={fieldState.invalid}
          className={horizontalFieldClassName(isHorizontal)}
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

export function EnumSelectField<
  TFieldValues extends FieldValues,
  TValue extends number,
>({
  control,
  name,
  label,
  options,
  isReadOnly,
  isHorizontal = false,
}: BaseFieldProps<TFieldValues> & {
  options: EnumOption<TValue>[];
}) {
  return (
    <Controller
      control={control}
      name={name}
      render={({ field, fieldState }) => (
        <Field
          orientation={isHorizontal ? "horizontal" : "vertical"}
          data-invalid={fieldState.invalid}
          className={horizontalFieldClassName(isHorizontal)}
        >
          <FieldLabel>{label}</FieldLabel>
          <Select
            value={numberToSelectValue(field.value)}
            onValueChange={(value) =>
              field.onChange(selectValueToNumber(value))
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
