# School edit tabs, plan and module switches, invoices tab Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Split the master's school edit screen into tabs (Profile, Plan & modules, Invoices), add a plan active switch, paid-module switches, a full invoices tab, and a Save bar for profile fields.

**Architecture:** Angular 22 standalone components in `src/Web/ClientApp/src/app/features/schools/`, using the generated `web-api-client.ts` (NSwag). One small backend change exposes `OverdueSince` on the invoice DTO. Every plan and module action calls the existing subscription endpoints; the server enforces the rules (ADR-012, ADR-014).

**Tech Stack:** .NET 10, EF Core 10, PostgreSQL, Angular 22 with Angular Material, Vitest (`ng test`), NUnit and Shouldly for backend tests, `$localize` with `ng extract-i18n`.

**Spec:** `docs/superpowers/specs/2026-11-14-school-edit-tabs-design.md` (all paths in this plan are relative to `Education Portal Full Stack App/`).

## Global Constraints

- Every user-visible string uses `i18n` or `$localize` with a custom id (`school.*`, `invoice.*`), and has Arabic and Urdu entries in `src/locale/messages.ar.json` and `messages.ur.json` in the same task (ENG-I18N). `translations.spec.ts` must pass.
- Templates and styles stay in external files (ENG-UI-18) for new components. Existing inline components in `features/schools/` may keep inline templates; do not move them unless the task says so.
- Logical CSS properties only (`margin-inline-start`, `text-align: start`).
- No plan cancellation: a plan action is `suspend` or `reactivate` only.
- No proration. Module switches never create an invoice.
- Module and subscription checks stay on the server. Angular hiding is not enough.
- `TreatWarningsAsErrors=true`; warnings fail the build.

## Review Focus

1. Master switches a core module off (must be refused by the server; UI shows it locked). Test: `SchoolModulesComponent`/module switch spec shows core switch disabled; server test already in Phase 4 suite, add one if missing.
2. Plan suspended while the user has unsaved profile edits. Test: Save bar state survives tab switch; leaving the page with dirty form asks for confirm.
3. Invoice with zero payments or with a partial balance. Test: balance due equals total when no payments; equals total minus sum when partial.
4. Invoice with no lines or payments yet. Test: tab shows an empty-state message, not a broken table.
5. Request fails (403, network). Test: a switch that fails reverts to its previous state and shows the server's problem message.

---

### Task 1: Expose `OverdueSince` on the invoice DTO

**Files:**
- Modify: `src/Application/Invoices/Common/InvoiceDto.cs`
- Modify: `src/Application/Invoices/Common/InvoiceReader.cs` (projection around line 15 and the mapping around line 29)
- Test: `tests/Application.FunctionalTests/Invoices/` (existing invoice query tests; add one case)

**Interfaces:**
- Consumes: `Invoice.OverdueSince` (`DateTimeOffset?`, `src/Domain/Entities/Invoice.cs:105`).
- Produces: `InvoiceDto` gains `DateTimeOffset? OverdueSince` after `PaidDate`. Generated client `InvoiceDto.overdueSince: Date | undefined` (regenerate with NSwag, `nswag.json`).

- [ ] **Step 1: Write the failing test**

Add to the invoice list functional test: an Open invoice whose `DueDate` has passed and which the lifecycle marks overdue returns `OverdueSince` equal to the marked time; an invoice that is not overdue returns `null`.

```csharp
[Test]
public async Task GetInvoices_ExposesOverdueSince()
{
    // Arrange: one overdue Open invoice (OverdueSince set), one not overdue.
    // Act: query the invoice list for the school.
    // Assert: overdue.OverdueSince == markedAt; other.OverdueSince is null.
}
```

- [ ] **Step 2: Run test to verify it fails**

Run: `dotnet test tests/Application.FunctionalTests --filter GetInvoices_ExposesOverdueSince`
Expected: FAIL (`OverdueSince` does not exist on `InvoiceDto`).

- [ ] **Step 3: Add the field**

Add `DateTimeOffset? OverdueSince` as the last positional parameter of `InvoiceDto`. Project `i.OverdueSince` in `InvoiceReader`'s projection and pass it in the mapping. Update every constructor call of `InvoiceDto` (search the solution).

- [ ] **Step 4: Run test to verify it passes**

Run: `dotnet test tests/Application.FunctionalTests --filter GetInvoices_ExposesOverdueSince`
Expected: PASS. Then run `dotnet build` (warnings fail it).

- [ ] **Step 5: Regenerate the client and commit**

Run the NSwag generation (see `nswag.json`) so `web-api-client.ts` gains `overdueSince`.

```bash
git add src/Application/Invoices tests/Application.FunctionalTests src/Web/ClientApp/src/app/web-api-client.ts
git commit -m "feat: expose overdue date on invoices"
```

---

### Task 2: Split the school edit screen into tabs

**Files:**
- Create: `src/Web/ClientApp/src/app/features/schools/school-edit.page.ts`, `school-edit.page.html`, `school-edit.page.css` (tab host, `mat-tab-group` with three tabs: Profile, Plan & modules, Invoices)
- Modify: `src/Web/ClientApp/src/app/features/schools/schools.page.ts` (route the edit view to the new host; keep behaviour)
- Modify: `src/Web/ClientApp/src/app/app.routes.ts` (tab selected via `?tab=` query param, per spec)
- Test: `src/Web/ClientApp/src/app/features/schools/school-edit.page.spec.ts`

**Interfaces:**
- Consumes: existing `SchoolFormComponent`, `SchoolSubscriptionComponent`, `SchoolModulesComponent`, and a new `InvoicesComponent` placeholder (Task 3).
- Produces: `SchoolEditPage` with input `school: SchoolSummaryDto` and a `tab` route param (`profile | plan | invoices`, default `profile`).

- [ ] **Step 1: Write the failing test**

```ts
it('shows the tab named in the tab query parameter', () => {
  // Arrange: route with ?tab=invoices
  // Act: create SchoolEditPage
  // Assert: the Invoices tab is selected; the Profile content is not rendered.
});
it('renders the three tabs in order: Profile, Plan & modules, Invoices', () => { /* assert labels */ });
```

- [ ] **Step 2: Run test to verify it fails**

Run: `npx ng test --include src/app/features/schools/school-edit.page.spec.ts` (from `src/Web/ClientApp`)
Expected: FAIL (`SchoolEditPage` not found).

- [ ] **Step 3: Implement the tab host**

Move the existing form into the Profile tab and the existing subscription and module components into the Plan & modules tab with no behaviour change. `mat-tab-group` with `selectedIndex` bound to the `tab` param; selecting a tab updates the query param. Labels use `i18n="@@school.tabs.profile"`, `@@school.tabs.plan`, `@@school.tabs.invoices`.

- [ ] **Step 4: Run test to verify it passes**

Run: `npx ng test --include src/app/features/schools/school-edit.page.spec.ts`
Expected: PASS.

- [ ] **Step 5: Translations and commit**

Run `npx ng extract-i18n`, add the three tab labels to `messages.ar.json` and `messages.ur.json`, run the `translations.spec.ts` test, then commit.

```bash
git add src/Web/ClientApp
git commit -m "feat: split school edit into tabs"
```

---

### Task 3: Invoices tab with totals, dates, payments and lines

**Files:**
- Create: `src/Web/ClientApp/src/app/features/schools/school-invoices.component.ts` (inline or external template per Global Constraints)
- Modify: `src/Web/ClientApp/src/app/features/schools/school-subscription.component.ts` and `.html` (remove the inline Invoices section; keep Record payment and Void actions by calling the same methods, now in the new component)
- Test: `src/Web/ClientApp/src/app/features/schools/school-invoices.component.spec.ts`

**Interfaces:**
- Consumes: `InvoicesClient.getInvoices(schoolId)` (returns `InvoiceDto[]`, now with `lines`, `payments`, `overdueSince`, `paidDate`), `InvoicesClient.createInvoice`, `recordPayment`, `voidInvoice`.
- Produces: `SchoolInvoicesComponent` with input `school: SchoolSummaryDto`.

- [ ] **Step 1: Write the failing tests**

```ts
it('shows subtotal, tax rate, tax and total for an expanded invoice', () => { /* ... */ });
it('shows balance due as total minus the sum of payments', () => {
  // total 255.00, payments 100.00 -> balance 155.00
});
it('shows balance equal to total when there are no payments', () => { /* ... */ });
it('shows an empty-state message when the invoice has no lines or payments', () => { /* ... */ });
it('shows the overdue date when overdueSince is set', () => { /* ... */ });
```

- [ ] **Step 2: Run tests to verify they fail**

Run: `npx ng test --include src/app/features/schools/school-invoices.component.spec.ts`
Expected: FAIL (component not found).

- [ ] **Step 3: Implement**

A table with number, status badge, total, balance due, issue date, due date, period, paid date. Each row expands to totals, lines and payments. Balance due is computed in the component from `total` and `payments`. Record payment and Void stay for Open invoices only. Dates use the `localDate` pipe; money uses `number: '1.2-2'` plus currency.

- [ ] **Step 4: Run tests to verify they pass**

Run: `npx ng test --include src/app/features/schools/school-invoices.component.spec.ts`
Expected: PASS. Then `npx ng test` for the whole schools folder to confirm the subscription component still passes after its Invoices section moved.

- [ ] **Step 5: Translations and commit**

Run `npx ng extract-i18n`, add `invoice.*` strings to `messages.ar.json` and `messages.ur.json`, run `translations.spec.ts`, then commit.

```bash
git add src/Web/ClientApp
git commit -m "feat: add invoices tab with totals, payments and lines"
```

---

### Task 4: Plan active switch and paid-module switches

**Files:**
- Modify: `src/Web/ClientApp/src/app/features/schools/school-subscription.component.ts` and `.html` (plan active switch; module switches replace the read-only list and the Add module and Remove buttons)
- Create: `src/Web/ClientApp/src/app/shared/components/confirm-dialog/confirm-dialog.component.ts` only if none exists (check first with `Glob` for `confirm-dialog`; reuse if found)
- Test: `src/Web/ClientApp/src/app/features/schools/school-subscription.component.spec.ts` (extend)

**Interfaces:**
- Consumes: `SubscriptionsClient`-style methods on `SchoolsClient`: `suspendSubscription(schoolId)`, `reactivateSubscription(schoolId)`, `addSubscriptionModule(schoolId, AddSubscriptionModuleCommand)`, `removeSubscriptionModule(schoolId, moduleCode)`.
- Produces: no new public API. Behaviour: plan switch off calls `suspendSubscription` after confirm; on calls `reactivateSubscription`. Module switch on calls `addSubscriptionModule`; off calls `removeSubscriptionModule` after confirm. Core modules render a disabled switch with a Core badge.

- [ ] **Step 1: Write the failing tests**

```ts
it('turning the plan switch off asks for confirm, then calls suspendSubscription', () => {});
it('turning the plan switch on calls reactivateSubscription', () => {});
it('core module switch is disabled and does not call the server', () => {});
it('turning a paid module off asks for confirm, then calls removeSubscriptionModule', () => {});
it('a failed module change reverts the switch and shows the server problem message', () => {
  // flush a 403 with MODULE_NOT_ENABLED; expect switch back to its previous state
});
```

- [ ] **Step 2: Run tests to verify they fail**

Run: `npx ng test --include src/app/features/schools/school-subscription.component.spec.ts`
Expected: FAIL on the new cases.

- [ ] **Step 3: Implement**

Use `mat-slide-toggle` (Material 3 switch). Optimistic toggle is not allowed: set the switch only after the server answers, and revert on error. Confirm before suspend and before removing a module. Keep the existing Extend and Change plan forms, moved into this tab.

- [ ] **Step 4: Run tests to verify they pass**

Run: `npx ng test --include src/app/features/schools/school-subscription.component.spec.ts`
Expected: PASS.

- [ ] **Step 5: Translations and commit**

Run `npx ng extract-i18n`, add the `school.plan.*` and `school.modules.*` strings to both locale files, run `translations.spec.ts`, then commit.

```bash
git add src/Web/ClientApp
git commit -m "feat: plan active and module switches on school edit"
```

---

### Task 5: Profile tab Save bar and unsaved-changes guard

**Files:**
- Modify: `src/Web/ClientApp/src/app/features/schools/school-form.component.ts` (dirty-state Save bar, Save and Cancel, helper text on each field)
- Create: `src/Web/ClientApp/src/app/features/schools/unsaved-changes.guard.ts` (route `canDeactivate`, asks for confirm when the Profile form is dirty)
- Modify: `src/Web/ClientApp/src/app/app.routes.ts` (attach the guard to the school edit route)
- Test: `src/Web/ClientApp/src/app/features/schools/unsaved-changes.guard.spec.ts` and extend `school-form.component.spec.ts`

**Interfaces:**
- Consumes: the existing school update call.
- Produces: `unsavedChangesGuard: CanDeactivateFn<HasUnsavedChanges>` where `HasUnsavedChanges` has `hasUnsavedChanges(): boolean`.

- [ ] **Step 1: Write the failing tests**

```ts
it('shows the Save bar only when the form is dirty', () => {});
it('Cancel restores the saved values and hides the Save bar', () => {});
it('guard returns false and asks for confirm when the form is dirty', () => {});
it('guard returns true when the form is clean', () => {});
```

- [ ] **Step 2: Run tests to verify they fail**

Run: `npx ng test --include src/app/features/schools/unsaved-changes.guard.spec.ts --include src/app/features/schools/school-form.component.spec.ts`
Expected: FAIL.

- [ ] **Step 3: Implement**

The Save bar is a bottom bar with Save and Cancel, shown only when `form.dirty`. Save is disabled while the form is invalid or saving. The guard is a `CanDeactivateFn`. Validation messages come from the server's ProblemDetails, shown inline.

- [ ] **Step 4: Run tests to verify they pass**

Run: `npx ng test --include src/app/features/schools/unsaved-changes.guard.spec.ts --include src/app/features/schools/school-form.component.spec.ts`
Expected: PASS.

- [ ] **Step 5: Translations and commit**

Run `npx ng extract-i18n`, add `school.save.*` strings to both locale files, run `translations.spec.ts`, then commit.

```bash
git add src/Web/ClientApp
git commit -m "feat: save bar and unsaved-changes guard for school profile"
```

---

## Final verification

- [ ] Run `dotnet build` and `dotnet test` from `Education Portal Full Stack App/`.
- [ ] Run `npx ng build` and `npx ng test` from `src/Web/ClientApp`.
- [ ] Run `python3 docs/ai/readme-guard.py --base origin/development`. Expected: no failure for the endpoint change, or `README: not needed - <reason>` in the PR description.
- [ ] Run `python3 docs/ai/review-checklist-guard.py --pr-body-file body.md` on the PR description with REVIEW-01 to REVIEW-06 ticked honestly.
