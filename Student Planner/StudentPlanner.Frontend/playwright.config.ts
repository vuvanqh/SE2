import { defineConfig, devices } from "@playwright/test";

/**
 * Playwright config. Boots the Vite dev server with a stubbed API base URL
 * pointing at our local mock backend. To run end-to-end tests, the user can
 * either:
 *   1) Run a real backend on localhost:5049 and run `npm run test:e2e`, or
 *   2) Run the bundled mock server via `node tests/e2e/mock-server.mjs`
 *      and then `npm run test:e2e`.
 *
 * Browsers must be installed first via:  npx playwright install chromium
 */
export default defineConfig({
    testDir: "./tests/e2e",
    timeout: 30_000,
    expect: { timeout: 5_000 },
    fullyParallel: false,
    retries: 0,
    reporter: [["list"]],
    use: {
        baseURL: "http://localhost:5173",
        trace: "on-first-retry",
        actionTimeout: 5_000,
    },
    projects: [
        {
            name: "chromium",
            use: { ...devices["Desktop Chrome"] },
        },
    ],
    webServer: [
        {
            // Tiny mock backend (express-free) for E2E tests.
            command: "node tests/e2e/mock-server.mjs",
            url: "http://localhost:5049/api/health",
            reuseExistingServer: !process.env.CI,
            stdout: "ignore",
            stderr: "pipe",
        },
        {
            command: "npm run dev -- --port 5173",
            url: "http://localhost:5173",
            reuseExistingServer: !process.env.CI,
            stdout: "ignore",
            stderr: "pipe",
            env: {
                VITE_API_BASE_URL: "http://localhost:5049",
            },
        },
    ],
});
