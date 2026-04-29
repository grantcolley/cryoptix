import { Category } from "./category";

export class Module {
  moduleId!: number;
  name!: string;
  icon!: string;
  permission!: string;
  categories: Category[] = [];
}
