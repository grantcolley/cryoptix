export function nullableTextToInputValue(
  value: string | null | undefined
): string {
  return value ?? "";
}

export function inputTextToNullable(value: string): string | null {
  const trimmed = value.trim();
  return trimmed.length === 0 ? null : trimmed;
}
