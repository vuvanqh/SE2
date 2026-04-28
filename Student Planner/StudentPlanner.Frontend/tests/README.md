# Frontend Test Suite

This folder contains all automated tests for the StudentPlanner frontend.

```
tests/
├── setup.ts                # Vitest setup (jsdom shims, MSW boot, global mocks)
├── test-utils.tsx          # render-with-providers helpers
├── mocks/
│   ├── handlers.ts         # MSW request handlers (one per API endpoint)
│   └── server.ts           # MSW node server
├── unit/                   # Pure / component-level unit tests
├── integration/            # Multi-component / hook-level tests with MSW
└── e2e/                    # Playwright end-to-end tests + mock backend
```

## Quickstart

```powershell
cd "Student Planner/StudentPlanner.Frontend"
npm install                 # if not already done
npm test                    # all unit + integration tests (Vitest)
npm run test:watch          # interactive watch mode
npm run test:coverage       # text + HTML coverage report

# E2E (browser-driven):
npx playwright install chromium
npm run test:e2e
npm run test:e2e:ui         # Playwright debugger UI
```

## Stack

| Layer            | Tool                              |
|------------------|-----------------------------------|
| Test runner      | Vitest 3                          |
| DOM environment  | jsdom                             |
| Component render | @testing-library/react            |
| User events      | @testing-library/user-event       |
| API mocking      | MSW (Mock Service Worker)         |
| E2E browser      | Playwright + Chromium             |

## Conventions

* Each test file ends in `.test.ts` / `.test.tsx` (Vitest) or `.spec.ts`
  (Playwright).
* MSW handlers live in `tests/mocks/handlers.ts`. Override per-test using
  `server.use(...)` for special cases.
* Tests must not rely on real network. The `onUnhandledRequest: "error"`
  setting will fail the test if you forget to mock an endpoint.
* `localStorage`, `sessionStorage`, and the global React-Query client are
  cleared between tests automatically (see `setup.ts`).

## What is covered

| Area                                           | Where                                            |
|------------------------------------------------|--------------------------------------------------|
| `helpers.ts` (validation / formatting)         | `unit/helpers.test.ts`                           |
| `apiClient` (token injection, 401 → refresh)   | `unit/apiClient.test.ts`                         |
| `<Input />` accessibility & error display      | `unit/Input.test.tsx`                            |
| `<RoleRoute />` access guard                   | `unit/RoleRoute.test.tsx`                        |
| `ModalContext` push/pop semantics              | `unit/ModalContext.test.tsx`                     |
| Toast notifications                            | `unit/toastNotifications.test.tsx`               |
| Login page (success / failure / disabled)      | `integration/LoginPage.test.tsx`                 |
| Registration page                              | `integration/RegisterPage.test.tsx`              |
| Forgot-password 2-step flow                    | `integration/ForgotPasswordPage.test.tsx`        |
| `EventForm` validation & submission            | `integration/EventForm.test.tsx`                 |
| `useAuth` login / logout / errors              | `integration/useAuth.test.tsx`                   |
| `useAdmin` user list / create / delete         | `integration/useAdmin.test.tsx`                  |
| `<ProtectedRoute />` redirect logic            | `integration/ProtectedRoute.test.tsx`            |
| Layered routing (Protected + Role)             | `integration/routing.test.tsx`                   |
| End-to-end auth & access control               | `e2e/auth.spec.ts`                               |
| End-to-end admin user list                     | `e2e/admin.spec.ts`                              |

## Bugs found while writing these tests

See `BUGS.md` at the repo's frontend root for the full list.
