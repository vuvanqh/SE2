# E2E Tests (Playwright)

These tests exercise the entire stack: a tiny Node mock backend + the Vite
dev server + a real browser.

## First-time setup

```powershell
cd "Student Planner/StudentPlanner.Frontend"
npx playwright install chromium
```

## Running

```powershell
npm run test:e2e          # headless
npm run test:e2e:ui       # Playwright UI
```

The Playwright config (`playwright.config.ts`) automatically boots:

1. `tests/e2e/mock-server.mjs` on `http://localhost:5049` (mock API)
2. `npm run dev -- --port 5173` (Vite dev server)

## Notes

* The mock server returns deterministic responses for the auth, admin, and
  faculty endpoints. Other endpoints return empty arrays / `{ ok: true }`.
* If you want to test against a real backend, set `VITE_API_BASE_URL` and
  remove the mock-server entry from `playwright.config.ts`.
