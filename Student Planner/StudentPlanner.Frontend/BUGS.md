# Frontend Bugs Found While Writing Tests

These bugs were discovered while building the unit / integration / E2E test
suite for the StudentPlanner frontend. Each entry describes the symptom, the
location, and the resolution.

---

## ✅ Fixed

### 1. `AcademicEventCard.tsx` imports a non-existent module *(build-blocking)*

* **File:** `src/features/events/components/AcademicEventCard.tsx`
* **Symptom:** `tsc -b` (run by `npm run build`) failed with
  `TS2307: Cannot find module '../../../types/academicEventTypes'`. The actual
  file is named `academic-event.types.ts`. The production build was broken.
* **Fix:** changed the import path to `../../../types/academic-event.types`.

### 2. `Input` component had no `id` on its `<input>` *(accessibility)*

* **File:** `src/components/common/Input.tsx`
* **Symptom:** the `<label htmlFor={id}>` had no matching `<input id=…>`,
  so screen readers and `getByLabelText`/`querySelector("[for]")` could not
  associate the label with the field. Also caused our integration tests to
  fail until fixed.
* **Fix:** added `id={id}` to the `<input>` element.

### 3. `EventForm` Cancel button submitted the form

* **File:** `src/components/common/EventForm.tsx`
* **Symptom:** the Cancel `<button>` was missing `type="button"`, so clicking
  it submitted the surrounding `<form>` and triggered validation/onSubmit on
  top of `onClose`. This is a regression risk for any inline modal form.
* **Fix:** added `type="button"` to Cancel and `type="submit"` to the primary
  button for clarity.

### 4. `getNEvents` returned the *furthest* upcoming events, not the soonest

* **File:** `src/api/helpers.ts`
* **Symptom:** the upcoming-events panel sorted events descending by start
  time before slicing, so the "next N events" actually showed the events
  *furthest* in the future.
* **Fix:** sort ascending (soonest first) and filter past events before
  slicing.

### 5. `useAuth().isAuthenticated` was non-reactive and forgot login on refresh

* **File:** `src/global-hooks/authHooks.tsx`
* **Symptom:** `isAuthenticated: !!queryClient.getQueryData(["user"])` was
  evaluated at hook-call time. After a hard refresh, the React Query cache is
  empty, so `ProtectedRoute` saw `isAuthenticated === false` and redirected
  the user to `/login` even though a valid token existed in `localStorage`.
* **Fix:** sourced the user via `useUser()` (which already falls back to
  `localStorage` through `getStoredUser`), making `isAuthenticated` both
  reactive and refresh-safe.

### 6. Forgot-password mutation leaked errors via toast (account enumeration)

* **File:** `src/global-hooks/authHooks.tsx`
* **Symptom:** the `sendResetToken` mutation had
  `onError: (error)=> errorMessage(error.message)`. The page intentionally
  swallows errors at the *page* layer to prevent account enumeration
  ("always advance to step 2"), but the mutation's onError still fired a
  toast leaking that the request failed (e.g. "User not found", network
  error, etc.). It also produced a confusing UX (toast + automatic
  step advance).
* **Fix:** removed the `onError` handler from `sendResetToken`. The page
  remains the sole error owner for this step.
* **Test:** `tests/integration/ForgotPasswordPage.test.tsx` — *"does not
  leak account-existence: failing token request still advances to step 2
  with no error toast"*.

---

## ⚠️ Still open (not fixed — flagging for the team)

### 7. `RoleRoute` redirects to `/unauthorized`, but no such route exists

* **File:** `src/components/routing/RoleRoute.tsx`
* **Symptom:** `<Navigate to="/unauthorized" replace />` is rendered when a
  role mismatch occurs, but `router.tsx` does not declare an `/unauthorized`
  route. The browser ends up on a blank URL with no rendered children.
* **Suggested fix:** add a public `Unauthorized.tsx` page and wire it up in
  the router, **or** redirect to `/login`.

### 8. `Input` component hard-codes `required`

* **File:** `src/components/common/Input.tsx`
* **Symptom:** `<input … required/>` — the `required` attribute is hard-coded
  *after* the prop spread, so callers cannot opt out by passing
  `required={false}`. As a result, optional fields like `description` /
  `location` in `EventForm` are forced required by the browser.
* **Suggested fix:** move `required` into the prop spread default:

  ```tsx
  <input id={id} name={id} className="input-field" required {...props}/>
  ```

  (then callers can pass `required={false}` for optional fields.)

### 9. `RegisterPage` carries a dead `confirmPassword` field

* **File:** `src/pages/public/RegisterPage.tsx`
* **Symptom:** the form state declares `confirmPassword: ""`, but no input
  for it is rendered and `registerRequest` does not include it. Dead state.
* **Suggested fix:** either render a real "Confirm Password" input and add
  client-side mismatch validation, or remove the dead state field.

### 10. `apiClient` skips token-refresh on any URL containing the literal `"login"`

* **File:** `src/api/apiClient.ts`
* **Code:** `request.url?.includes("login")`
* **Symptom:** any request whose URL happens to contain `login` (e.g. a
  hypothetical `/audit/last-login`) will not auto-refresh on 401. Low
  severity today, but a foot-gun.
* **Suggested fix:** match the exact path: `request.url?.endsWith("/auth/login")`.

### 11. `useAuth` mutates the global `queryClient` singleton

* **File:** `src/global-hooks/authHooks.tsx`
* **Symptom:** `queryClient.setQueryData(["user"], data)` and
  `queryClient.getQueryData(...)` use the imported singleton, ignoring the
  React Query *context*. This makes the hook hard to test in isolation and
  prevents per-feature query clients (e.g. for storybook or test harnesses).
* **Suggested fix:** use `useQueryClient()` from `@tanstack/react-query`
  inside the hook.

### 12. `RegisterPage` does not respect API errors / always shows "invalid credentials"

* **File:** `src/pages/public/RegisterPage.tsx`
* **Symptom:** the `catch` branch always prepends the literal string
  `"invalid credentials"` to the errors list, which is meaningless during
  registration. The user receives a misleading error.
* **Suggested fix:** use `extractErrors(err)` (already imported elsewhere)
  to surface real API messages, e.g. *"Email already registered"*.

### 13. `LoginPage` `onError` toasts `error.message` (raw axios string)

* **File:** `src/global-hooks/authHooks.tsx`
* **Symptom:** `onError: (error)=> {errorMessage(error.message)}` shows
  axios's generic `Request failed with status code 401` to the user.
* **Suggested fix:** call `extractErrors(error).join(" ")` so the toast
  shows the actual server message.

### 14. `useMyEventRequests` and `useAllEventRequests` share the same query key

* **File:** `src/features/eventRequests/hooks/eventRequestHooks.tsx`
* **Symptom:** both hooks use `queryKey: ["eventRequests", "all"]`. As a
  result, when a Manager and an Admin both mount in (say, a tabbed admin
  panel), they collide — the second mount sees the cached data of the
  first and the `queryFn` is not re-run. Worse, calls to
  `queryClient.invalidateQueries({queryKey: ["eventRequests", "all"]})`
  invalidate the wrong cache.
* **Suggested fix:** distinct keys, e.g. `["eventRequests", "mine"]` and
  `["eventRequests", "all"]`.
