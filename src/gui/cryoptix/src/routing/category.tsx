import { Page } from "./page";

export class Category {
  categoryId!: number;
  name!: string;
  icon!: string;
  permission!: string;
  pages: Page[] = [];
}
