# School edit: plan, modules, invoices and tabs (design)

Date: 2026-11-14. Status: approved by the maintainer in conversation. Size: a project (several PRs).

## Intent and why

The master tenant edits a school today on one long page. Plan, module and invoice actions are mixed together, invoices are a short inline list with no totals or payment history, and module state is read-only. The master wants to:

1. Activate and deactivate a school's plan directly from the school edit screen.
2. See invoices on their own tab, with the information needed to chase and reconcile them.
3. Turn a school's paid modules on and off from the school edit screen.
4. Make the screen easier to edit, following Google's Material Design 3 guidance.

## Goals

- A school's plan can be suspended and reactivated from its edit screen, with a clear status.
- Paid modules are switched on and off from the same screen, with core modules shown as locked.
- Invoices have their own tab with totals, tax, dates, payments and lines.
- The school edit screen is split into tabs, with a Save bar for profile fields and an unsaved-changes warning.
- Every new string is translated to English, Arabic and Urdu (ENG-I18N).

## Non-goals

- No new plan cancellation (no "Cancelled" status). Deactivate means suspend.
- No proration or credit when plans or modules change (ADR-014 stays as it is).
- No change to the school user's add-on request flow (ADR-014).
- No card or gateway payment.

## Anti-goals

- Hiding a control in Angular without the server refusing the action. Every action is still checked on the server (ADR-012, `MODULE_NOT_ENABLED`, 403).
- Letting the master switch a core module off. Core modules are always on.
- A module toggle that silently changes billing. Turning a paid module on does not create an invoice by itself.

## Constraints

- ADR-012: `GET /api/Schools/{id}/modules` is read only. Paid modules are on only through the subscription, so the module switches call the subscription module endpoints (`POST`/`DELETE` on `/api/Schools/{id}/subscription/modules`, to be confirmed in the generated client).
- ADR-014: suspend, reactivate, plan change and module changes take effect at once, with no proration, and removed modules are expired, not deleted. Each change is audited as `SubscriptionChanged`.
- ADR-016: every commercial change is audited through `ChangeAuditInterceptor`. No bulk updates on those tables.
- AGENTS.md section 6: every user-visible string is translatable; Arabic and Urdu translations ship in the same PR. Logical CSS properties only (RTL).
- AGENTS.md section 2: a module's controls are checked on the server. Angular hiding is not enough.
- Components keep their template and styles in external files (ENG-UI-18).
- The maintainer's theme in `Theme Angular/` is the guide for components and tokens (ENG-UI-17).

- **Decided (2026-11-14, maintainer):** every school has one main branch, created with it. `TenantModule.BranchId` is required. A grant on the main branch is school-wide (`IsSchoolWide`), and a grant on any other branch covers that branch only. Entitlements without a branch count only school-wide grants (fail closed).

## Approaches considered

1. **Deactivate = suspend (chosen).** Uses the existing `suspend` and `reactivate` endpoints. No backend change. Reversible, and matches "deactivate" in the maintainer's words.
2. **New Cancelled status.** Needs a new endpoint, a new subscription status, an ADR and a billing rule for the end date. Rejected for this step; it can come later as its own ADR if needed.

For module toggles:

1. **Immediate through the subscription (chosen).** Switch on adds the module, switch off expires it, with a confirm step. Matches ADR-014 and the existing "Add module" and "Remove" actions.
2. **Add-on request flow.** Switch on creates an `AddOnRequest` for the master to approve. Rejected: the master is the actor here, so a request to self-approve adds steps without a benefit.

For editing:

1. **Tabs with a Save bar for profile fields (chosen).** Plan and module actions are immediate with confirm dialogs for risky ones, because the server applies them at once. Profile fields are edited in place and saved with Save and Cancel.
2. **Edit mode for each tab.** Rejected: plan and module actions do not need a second save step, and it would hide their immediate effect.

## Parts

### 1. Invoices tab (backend small change, then frontend)

- Backend: add `OverdueSince` to `InvoiceDto` and `InvoiceReader` (the database field exists; the API does not expose it). Unit and functional tests for the new field.
- Frontend: a new `Invoices` tab with one row per invoice (number, status, total, balance due, issue date, due date, period, paid date). Expanding a row shows the totals (subtotal, tax rate, tax, total), the lines and the payments list (date, method, reference, amount). Record payment and Void stay as row actions, with the existing guards (only Open invoices).
- Balance due = total minus the sum of payments. Shown in the school currency.

### 2. Plan and modules on the school edit screen

- "Plan & modules" tab shows the plan name, status badge, billing cycle, price, period end.
- Activate/deactivate: a single switch labelled "Plan active". Off calls `suspend` (with confirm); on calls `reactivate`. Status badge updates from the response.
- Modules: a list of every module with a switch. Core modules show a disabled, locked switch with a Core badge. Paid modules are on or off; switching on calls the subscription add endpoint, switching off asks for confirmation then calls the remove endpoint.
- Plan change and extend stay as controls on this tab, moved from the current page.

### 3. Profile tab and Save bar

- "Profile" tab holds the school's editable fields (name, domain, country, active switch, branding entry point).
- A Save bar appears only when the form is dirty. Save and Cancel; leaving with unsaved changes shows a confirm.
- Switches for Active and other on/off values; text fields with helper text and inline errors; validation messages come from the server (`ValidationMessages`).

### 4. Translations and accessibility

- New strings get `i18n` ids (`school.tabs.*`, `school.plan.*`, `school.modules.*`, `invoice.*`), then `npx ng extract-i18n`, then Arabic and Urdu entries in `messages.ar.json` and `messages.ur.json`.
- Tabs and switches must be keyboard usable and announce their state (WCAG 2.2 AA).

## Testing

- Backend: unit tests for the `OverdueSince` mapping; functional test that the invoice list returns `OverdueSince`. Existing tests for suspend, reactivate and module changes stay as they are.
- Denial tests: a non-master user is refused on each subscription action (already covered by the master-only policy; add a test for any new endpoint).
- Frontend: component specs for each tab; the Save bar appears only when dirty; core module switch is disabled; confirm dialog before suspend and before removing a module.
- Acceptance: the master can suspend a school from its edit screen and the school's users see `MODULE_NOT_ENABLED`.

## Left to the builder (my calls, marked)

- Exact route names for the tabs (for example `/schools/:id/edit?tab=invoices`). My call: query param, so the tab survives a reload.
- Exact confirm-dialog wording. My call: short, names the school and the module.
- Whether the Save bar also covers branding. My call: no; branding keeps its own editor.

## Order of work

Each step is one reviewable PR into `development`.

1. Backend: `OverdueSince` in the invoice DTO, with tests.
2. Frontend: split the school edit screen into tabs (Profile, Plan & modules, Invoices), moving existing controls without behaviour change.
3. Frontend: Invoices tab with the new fields.
4. Frontend: plan activate/deactivate switch and module switches with confirm dialogs.
5. Frontend: Save bar and unsaved-changes guard on the Profile tab.
6. Translations for every step (Arabic and Urdu in the same PR as each step).
