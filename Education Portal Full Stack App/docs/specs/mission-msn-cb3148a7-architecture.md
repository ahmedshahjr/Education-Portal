# Architecture: entities in the UI, trial banner, toasts and campus filter

Mission `msn_cb3148a74d68cbf6ccc3e5d2`, task `t3` (Architecture). Builds on the specification (`mission-msn-cb3148a7-ui-entities.md`, AC ids) and the pseudocode (`mission-msn-cb3148a7-pseudocode.md`). Open questions OQ1 to OQ5 are still open; this design holds under the assumptions stated in those documents.

## 1. Modules

Frontend only, except where a row says otherwise. Nothing here crosses the Clean Architecture layers (AGENTS.md section 4).

| Module | Location | Responsibility | Status |
|---|---|---|---|
| Entity screens | `src/app/features/<entity>/` | List screen per entity (AC1.x) | Exists for school, campus, people, roles, modules; others pending OQ1 |
| Trial banner | `src/app/shared/components/trial-banner/` | Shows days left during a trial (AC2.x) | Exists; gains a `visible` rule and `daysLeft` |
| Toast service | `src/app/shared/services/toast.service.ts` | One place for snackbar duration and text (AC3.x) | New |
| Campus filter | `src/app/shared/components/campus-filter/` | Campus dropdown, shown for two or more campuses (AC4.x) | New |
| Module guard | `src/app/core/modules/` | Client menu and route mirror of server module checks | Exists |
| Authorization pipeline | `src/Application/Common/Behaviours/AuthorizationBehaviour.cs` | Server check of module, role and tenant (AC1.4, AC4.3) | Exists |

## 2. Interfaces

```ts
// shared/services/toast.service.ts (new)
export interface ToastService {
  success(messageKey: string): void;                    // AC3.1, fixed 3000 ms
  error(problem: unknown): void;                        // AC3.2, uses problemMessages()
}

// shared/components/campus-filter/campus-filter.component.ts (new)
// inputs:  campuses: readonly CampusDto[]               // from GET /api/Campuses (OQ5)
// outputs: selected: string | null                      // null = "All campuses"
// rule:    rendered only when campuses.length > 1       // AC4.1

// shared/components/trial-banner/trial-banner.component.ts (changed)
// inputs:  subscription: SubscriptionDto | null, isMaster: boolean
// rule:    visible = !isMaster && subscription?.isTrial === true   // AC2.1, AC2.2, AC2.4
// pure fn: daysLeft(trialEnd: Date, now: Date): number  // AC2.3, floor, min 0

// Entity list screens share one shape (new, used by every list screen):
// state: 'loading' | 'ready' | 'empty' | 'error'         // AC1.2, AC1.3
```

Server interfaces are unchanged. The campus list is `GET /api/Campuses`, which already exists (`Web/Endpoints/Campuses.cs:25`); whether it returns all of a school's campuses or only the user's is OQ5.

## 3. Data flow

```
Browser                                              Server
-------                                              ------
app-layout (every page)
  ├─ subscription state  ── GET /api/Subscriptions/mine ──► handler, [Authorize], school from host
  │    └─ trial-banner (above router-outlet, AC2.1)
  ├─ module state        ── GET /api/Modules/enabled ──► GetEnabledModules, school from host
  │    └─ moduleGuard / menu (AC1.1, AC1.4 client side)
  │
entity list screen
  ├─ campuses            ── GET /api/Campuses ──────────► GetCampuses, [Authorize]
  │    └─ campus-filter (only if > 1, AC4.1)
  ├─ rows                ── GET /api/<entity> ──────────► handler: module check, tenant filter,
  │    └─ state: loading | ready | empty | error           EF global filters (SchoolId, CampusId)
  │         (error: problemMessages, AC1.3)
  │
create / update / delete
  └─ success ─► toast.success(key)   (AC3.1)
     error   ─► toast.error(problem) (AC3.2)
```

Campus selection (AC4.2 and AC4.4): the filter is client-side for rows already loaded in full. When a screen is campus-scoped, the chosen campus goes into the route (`/campuses/{campusId}/...`); the server still checks membership on every request (AC4.3).

## 4. Decisions

- **D1 Frontend only for R2 to R4.** The banner, toasts and filter need no new endpoint. The campus list already exists.
- **D2 Toasts through one service.** Spreading `MatSnackBar.open` across 18 files makes the duration and wording inconsistent. One service keeps AC3.1 and AC3.2 testable in one place.
- **D3 Banner in the layout, not per page.** It is rendered once above `router-outlet` in `app-layout`, so it covers every screen (AC2.1) without each page repeating it.
- **D4 Client filter first, server filter later.** Filtering in memory is enough while lists are loaded whole. Server-side filtering is added only for paged lists, and that change needs its own decision.
- **D5 Server stays the authority.** The client hides menus, banner and filter by state; the server refuses disabled modules (`MODULE_NOT_ENABLED`), foreign campuses and foreign schools. Client state is never a security control (AGENTS.md section 2).
- **D6 Entity screens wait for OQ1.** The shape is fixed now (the four-state list). The set of screens is not, because building screens without backends would invent requirements.

## 5. Records and follow-up

- Decisions D1 to D6 are recorded here. The repo's change-management rules (`docs/ai/secure-compliance/change-management/SKILL.md`) require an ADR for a design decision; that ADR should be written before implementation, not in this task.
- Verification: the document was checked against the code paths it names (`app-layout.component.ts:50`, `Web/Endpoints/Campuses.cs:25`, `AuthorizationBehaviour.cs`, the existing `trial-banner` and `shared/components` folders). No code changed and no tests were run.
