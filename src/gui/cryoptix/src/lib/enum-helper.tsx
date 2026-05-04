export type EnumOption<TValue extends number> = {
  value: TValue;
  label: string;
};

export function enumToOptions<const TEnum extends Record<string, number>>(
  enumObject: TEnum,
  labels: Record<TEnum[keyof TEnum], string>
): EnumOption<TEnum[keyof TEnum]>[] {
  return Object.values(enumObject).map((value) => ({
    value: value as TEnum[keyof TEnum],
    label: labels[value as TEnum[keyof TEnum]],
  }));
}
