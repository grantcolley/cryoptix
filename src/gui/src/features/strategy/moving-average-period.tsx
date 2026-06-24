import type { Control, FieldPath, FieldValues, Path } from "react-hook-form";
import {
  MovingAverageSmoothingType,
  MovingAverageSmoothingTypeLabels,
} from "@/features/api/schema/moving-average-smothing-type";
import {
  EnumSelectField,
  IntegerField,
  TextField,
} from "@/features/strategy/strategy-form-fields";
import { enumToOptions } from "@/lib/enum-helper";

const movingAverageSmoothingTypeOptions = enumToOptions(
  MovingAverageSmoothingType,
  MovingAverageSmoothingTypeLabels
);

type MovingAveragePeriodProps<TFieldValues extends FieldValues> = {
  control: Control<TFieldValues>;
  name: FieldPath<TFieldValues>;
  isReadOnly: boolean;
  isHorizontal?: boolean;
};

export function MovingAveragePeriod<TFieldValues extends FieldValues>({
  control,
  name,
  isReadOnly,
  isHorizontal = false,
}: MovingAveragePeriodProps<TFieldValues>) {
  return (
    <div className="flex flex-col gap-3 rounded-lg border p-3">
      <TextField
        control={control}
        name={`${name}.name` as Path<TFieldValues>}
        label="Name"
        isReadOnly={isReadOnly}
        isHorizontal={isHorizontal}
      />
      <IntegerField
        control={control}
        name={`${name}.value` as Path<TFieldValues>}
        label="Value"
        isReadOnly={isReadOnly}
        isHorizontal={isHorizontal}
      />
      <EnumSelectField
        control={control}
        name={`${name}.smoothingType` as Path<TFieldValues>}
        label="Type"
        options={movingAverageSmoothingTypeOptions}
        isReadOnly={isReadOnly}
        isHorizontal={isHorizontal}
      />
    </div>
  );
}

export default MovingAveragePeriod;
