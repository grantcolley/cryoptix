import type { Control, FieldPath, FieldValues, Path } from "react-hook-form";
import { Button } from "@/components/ui/button";
import {
  Tooltip,
  TooltipContent,
  TooltipTrigger,
} from "@/components/ui/tooltip";
import { Icon } from "@/components/icon/icon";
import { icons } from "@/components/icon/icons";
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
  onRemove?: () => void;
};

export function MovingAveragePeriod<TFieldValues extends FieldValues>({
  control,
  name,
  isReadOnly,
  isHorizontal = false,
  onRemove,
}: MovingAveragePeriodProps<TFieldValues>) {
  const handleRemove = () => {
    onRemove?.();
  };

  const removeButton = !isReadOnly ? (
    <Tooltip>
      <TooltipTrigger asChild>
        <Button
          type="button"
          variant="ghost"
          size="icon"
          onClick={handleRemove}
          aria-label="Remove moving average"
          className="size-7 p-0"
        >
          <Icon icon={icons.x} />
        </Button>
      </TooltipTrigger>
      <TooltipContent>Remove moving average</TooltipContent>
    </Tooltip>
  ) : null;

  return (
    <div className="flex flex-col gap-3 rounded-lg border p-3">
      <TextField
        control={control}
        name={`${name}.name` as Path<TFieldValues>}
        label="Name"
        isReadOnly={isReadOnly}
        isHorizontal={isHorizontal}
        labelAction={removeButton}
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
