/// <reference types="vitest" />
import { defineConfig } from "vitest/config";
import react from "@vitejs/plugin-react";

export default defineConfig({
    plugins: [react()],
    define: {
        // Provide a deterministic API base in tests so MSW can intercept.
        "import.meta.env.VITE_API_BASE_URL": JSON.stringify("http://localhost:5049"),
    },
    test: {
        environment: "jsdom",
        globals: true,
        setupFiles: ["./tests/setup.ts"],
        css: false,
        include: ["tests/**/*.test.{ts,tsx}"],
        exclude: ["tests/e2e/**", "node_modules/**"],
        coverage: {
            provider: "v8",
            reporter: ["text", "html"],
            include: ["src/**/*.{ts,tsx}"],
            exclude: [
                "src/**/*.d.ts",
                "src/main.tsx",
                "src/types/**",
                "src/assets/**",
                "src/styles/**",
            ],
        },
    },
});
