import js from "@eslint/js";
import tseslint from "typescript-eslint";
import react from "eslint-plugin-react";
import reactHooks from "eslint-plugin-react-hooks";
import reactRefresh from "eslint-plugin-react-refresh";
import prettier from "eslint-config-prettier";

export default tseslint.config(
  // Ignore build output
  {
    ignores: ["dist", "build", "node_modules"],
  },

  // Base JS rules
  js.configs.recommended,

  // TypeScript (type-aware)
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
      // React hooks rules
      ...reactHooks.configs.recommended.rules,

      // Vite fast refresh rule
      "react-refresh/only-export-components": "warn",

      // Enforce Icon wrapper (no direct lucide usage)
      "@typescript-eslint/no-restricted-imports": [
        "error",
        {
          paths: [
            {
              name: "lucide-react",
              message:
                "Do not import icons directly from lucide-react. Use '@/components/ui/icon' instead.",
              allowTypeImports: true,
            },
          ],
        },
      ],
    },
  },

  // Disable formatting conflicts with Prettier
  prettier
);
