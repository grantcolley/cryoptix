import js from "@eslint/js";
import tseslint from "typescript-eslint";
import react from "eslint-plugin-react";
import reactHooks from "eslint-plugin-react-hooks";
import reactRefresh from "eslint-plugin-react-refresh";
import prettier from "eslint-config-prettier";

export default tseslint.config(
  {
    ignores: ["dist", "build", "node_modules"],
  },

  js.configs.recommended,

  ...tseslint.configs.recommendedTypeChecked,

  {
    files: ["**/*.{ts,tsx}"],

    languageOptions: {
      parserOptions: {
        projectService: true,
        tsconfigRootDir: import.meta.dirname,
      },
    },

    plugins: {
      react,
      "react-hooks": reactHooks,
      "react-refresh": reactRefresh,
    },

    settings: {
      react: {
        version: "detect",
      },
    },

    rules: {
      ...reactHooks.configs.recommended.rules,

      "react-refresh/only-export-components": "warn",
    },
  },

  // shadcn/ui files intentionally export helpers/constants alongside components.
  {
    files: ["src/components/ui/**/*.{ts,tsx}"],
    rules: {
      "react-refresh/only-export-components": "off",
    },
  },

  // Restrict direct lucide-react imports in app code only.
  // Allows shadcn/ui components and your central icon registry.
  {
    files: ["src/**/*.{ts,tsx}"],
    ignores: [
      "src/components/icon/**/*.{ts,tsx}",
      "src/components/ui/**/*.{ts,tsx}",
    ],

    rules: {
      "@typescript-eslint/no-restricted-imports": [
        "error",
        {
          paths: [
            {
              name: "lucide-react",
              message:
                "Do not import icons directly from lucide-react. Use '@/components/icon' instead.",
              allowTypeImports: true,
            },
          ],
        },
      ],
    },
  },

  prettier
);
