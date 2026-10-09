import { useWatch, type Control, type UseFormSetValue } from "react-hook-form";
import type {
  Strategy,
  StrategyFormValues,
} from "@/features/api/schema/strategy-schema";
import {
  IndicatorDefinitions,
  getDefaultIndicatorValues,
} from "@/features/api/schema/indicator-definitions";
import { IndicatorValueTypeLabels } from "@/features/api/schema/indicator-value-type";
import { Button } from "@/components/ui/button";
import {
  Tooltip,
  TooltipContent,
  TooltipTrigger,
} from "@/components/ui/tooltip";
import { Icon } from "@/components/icon/icon";
import { icons } from "@/components/icon/icons";
import {
  IndicatorType,
  IndicatorTypeLabels,
} from "@/features/api/schema/indicator-type";
import {
  EnumSelectField,
  IntegerField,
  TextField,
} from "@/features/strategy/strategy-form-fields";
import { enumToOptions } from "@/lib/enum-helper";

const indicatorTypeOptions = enumToOptions(IndicatorType, IndicatorTypeLabels);

type IndicatorPath = `indicators.${number}`;

type IndicatorProps = {
  control: Control<StrategyFormValues, unknown, Strategy>;
  setValue: UseFormSetValue<StrategyFormValues>;
  name: IndicatorPath;
  isReadOnly: boolean;
  isHorizontal?: boolean;
  onRemove?: () => void;
};

export function Indicator({
  control,
  setValue,
  name,
  isReadOnly,
  isHorizontal = false,
  onRemove,
}: IndicatorProps) {
  const indicatorType =
    useWatch({
      control,
      name: `${name}.indicatorType`,
    }) ?? IndicatorType.Sma;
  const definition = IndicatorDefinitions[indicatorType];
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
          aria-label="Remove indicator"
          className="size-7 p-0"
        >
          <Icon icon={icons.x} />
        </Button>
      </TooltipTrigger>
      <TooltipContent>Remove indicator</TooltipContent>
    </Tooltip>
  ) : null;

  return (
    <div className="flex flex-col gap-3 rounded-lg border p-3">
      <TextField
        control={control}
        name={`${name}.name`}
        label="Name"
        isReadOnly={isReadOnly}
        isHorizontal={isHorizontal}
        labelAction={removeButton}
      />
      {definition.values.map((valueDefinition, index) => (
        <IntegerField
          key={valueDefinition.type}
          control={control}
          name={`${name}.values.${index}.value`}
          label={IndicatorValueTypeLabels[valueDefinition.type]}
          isReadOnly={isReadOnly}
          isHorizontal={isHorizontal}
        />
      ))}
      <EnumSelectField
        control={control}
        name={`${name}.indicatorType`}
        label="Type"
        options={indicatorTypeOptions}
        onValueChange={(newIndicatorType) => {
          setValue(
            `${name}.name`,
            `New ${IndicatorTypeLabels[newIndicatorType]}`,
            {
              shouldDirty: true,
              shouldValidate: true,
            }
          );
          setValue(
            `${name}.values`,
            getDefaultIndicatorValues(newIndicatorType),
            {
              shouldDirty: true,
              shouldValidate: true,
            }
          );
        }}
        isReadOnly={isReadOnly}
        isHorizontal={isHorizontal}
      />
    </div>
  );
}

export default Indicator;
