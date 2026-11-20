# Specification: entities in the UI, trial banner, toasts and campus filter

Mission `msn_cb3148a74d68cbf6ccc3e5d2`, task `t1` (Specification). Status: draft for review.

## 1. Requirements

| ID | Requirement | Source |
|---|---|---|
| R1 | Every core entity of the product is reachable from the UI with a list screen. | Mission: "add all entities in the ui" |
| R2 | When the school's trial is active, a trial banner is shown above the page content on every screen. | Mission: "show trail banner above the ui if trail is active" |
| R3 | Actions report their outcome with a toast (snackbar). | Mission: "add toaster from messager" |
| R4 | When the school has more than one campus, a campus dropdown above the list filters the rows by campus. | Mission: "if campuses are multiple more than 1 add dropdown" |

## 2. Reading of the request (to confirm)

- **"all entities"** is read as the core entities in AGENTS.md section 1: School, Campus, Student, Teacher, Parent, Admin/staff user, Course, Enrolment, Grade. Only School, Campus, staff (`UserDetails`) and modules exist today; Student, Course, Enrolment, Grade, Parent are not built. The UI for entities that do not exist cannot be added without their backend, so they are out of scope for this mission unless the owner says otherwise. **Open question OQ1.**
- **"trail"** is read as the free trial (`Subscription` in trial, see `SchoolRegistration.TrialDays`). The existing `app-trial-banner` component already renders it in the app layout (`shared/components/app-layout/app-layout.component.ts:50`). R2 means it must sit above the page content and be visible on every screen while the trial is active, and be hidden when it is not. **OQ2:** confirm this reading.
- **"toaster from messager"** is read as Angular Material `MatSnackBar`, which the app already uses (18 files). **OQ3:** confirm.
- **"campuses multiple"** is read as: the school has more than one campus. The dropdown lists "All campuses" plus each campus. **OQ4:** confirm whether the filter applies to every list or only to campus-scoped lists (students, staff, enrolments).

## 3. Acceptance criteria

Each criterion is testable. "Checked by" names who verifies it; the author may not verify their own criteria (mission rule).

### R1 entities in the UI

- AC1.1: The master portal menu and each school menu contain a list screen for every entity that has a backend endpoint. Checked by: a navigation spec asserting the item ids.
- AC1.2: Each list screen loads its rows from the API and shows an empty state when there are none. Checked by: a component spec per screen, mocking the client.
- AC1.3: A list screen shows an error message when the API returns a 4xx or 5xx response. Checked by: a component spec flushing an error response.
- AC1.4: Module-gated screens use `moduleGuard` and server-side `[Authorize(Module=…)]` (AGENTS.md section 2). Checked by: a navigation spec for a school without the module, and a functional test that a direct API call is refused with `MODULE_NOT_ENABLED`.

### R2 trial banner

- AC2.1: While the school's subscription is in trial, the banner is rendered above the page content on every routed page. Checked by: an app-layout spec with a trial subscription.
- AC2.2: The banner is not rendered when the subscription is not a trial, or when there is no subscription. Checked by: the same spec with those states.
- AC2.3: The banner shows the number of days left (`TrialEndDate` − now, rounded down, minimum 0). Checked by: a unit test of the day calculation at the boundaries: exactly now, one day, and after the end.
- AC2.4: The banner is hidden for the master tenant. Checked by: an app-layout spec as master.

### R3 toasts

- AC3.1: Every successful create, update and delete shows a snackbar with a translated message, for 3 seconds. Checked by: component specs that spy on `MatSnackBar.open`.
- AC3.2: Every failed request shows a snackbar with the first problem message. Checked by: the same specs, with an error response.
- AC3.3: Every snackbar text is translated in en, ar and ur (AGENTS.md section 6). Checked by: `translations.spec.ts`.

### R4 campus filter

- AC4.1: The dropdown is shown only when the school has more than one campus. Checked by: a spec with 1 campus (no dropdown) and 2 campuses (dropdown).
- AC4.2: Choosing a campus filters the rows client-side to that campus; "All campuses" shows every row. Checked by: a spec counting rows after each choice.
- AC4.3: The campus list comes from the server, never from the browser's own data. The server still checks the user belongs to the campus on every request (AGENTS.md section 2). Checked by: a functional test that a campus the user does not belong to is refused.
- AC4.4: The selected campus is kept in the route (`/campuses/{campusId}/...`) when the list is campus-scoped (AGENTS.md section 2). Checked by: a router spec.

### Cross-cutting

- AC5.1: `dotnet build` and `npm run build` pass with warnings as errors. Checked by: CI.
- AC5.2: Every new use case has a functional test for another school, another campus and a disabled module (AGENTS.md section 6). Checked by: `security-testing` review.
- AC5.3: No new test is removed or skipped to make the suite pass.

## 4. Design (modules and interfaces)

- `shared/components/trial-banner`: already exists. Add a `visible` check for the master tenant and a pure `daysLeft(trialEnd, now)` function, unit tested (AC2.3, AC2.4).
- `shared/components/campus-filter` (new): an `input` of campuses and an `output` of the selected campus id or `null`. Hidden when fewer than two campuses (AC4.1).
- `shared/services/toast`: a thin wrapper over `MatSnackBar` with `success(message)` and `error(problem)`. Used by the features so the 3-second duration is set in one place (AC3.1, AC3.2).
- No new backend endpoints are needed for R2 and R3. R4 needs the campus list, which `GET /api/Campuses` already gives (to be confirmed, OQ5).

## 5. Decisions and risks

- D1: Filtering by campus is client-side for lists already loaded in full; server-side filtering is added only where a list is paged (AC4.2).
- Risk: the reading of "all entities" (OQ1) changes the size of the work by an order of magnitude. It must be settled before implementation.
- Risk: the trial banner is currently in the layout; moving it above the content must not change the layout of the sidebar (checked visually, not by a spec).

## 6. Open questions

- OQ1: Which entities? Only those with backends, or should missing ones get backend work too?
- OQ2: Is "trail" the free trial?
- OQ3: Is "toaster from messager" `MatSnackBar`?
- OQ4: Does the campus filter apply to every list, or only to campus-scoped lists?
- OQ5: Does `GET /api/Campuses` return the campuses of the signed-in user only, or all the school's campuses?
