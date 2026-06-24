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
};

export function MovingAveragePeriod<TFieldValues extends FieldValues>({
  control,
  name,
  isReadOnly,
}: MovingAveragePeriodProps<TFieldValues>) {
  return (
    <div className="grid grid-cols-1 gap-3 rounded-lg border p-3 lg:grid-cols-3">
      <TextField
        control={control}
        name={`${name}.name` as Path<TFieldValues>}
        label="Name"
        isReadOnly={isReadOnly}
      />
      <IntegerField
        control={control}
        name={`${name}.value` as Path<TFieldValues>}
        label="Value"
        isReadOnly={isReadOnly}
      />
      <EnumSelectField
        control={control}
        name={`${name}.smoothingType` as Path<TFieldValues>}
        label="Type"
        options={movingAverageSmoothingTypeOptions}
        isReadOnly={isReadOnly}
      />
    </div>
  );
}

export default MovingAveragePeriod;
