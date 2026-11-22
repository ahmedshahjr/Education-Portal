# Design: per-campus rights, tenants, users and roles, change history, calendar

Companion to `rights-tenants-history-calendar.md` (requirements R1.1 to R5.3). Mission task t3. Layers follow
Domain, Application, Infrastructure, Web, Angular.

## Phase 1: rights foundation

### Finding that shapes the design

The request rules (`RequestRules`, ADR-025) run before the handler and see only the request type, not a campus. Campuses arrive as request
fields, and handlers already call `ICampusAccess.EnsureCampusAsync(campusId)`. So the request rules cannot know the campus.
Decision: **the request rules ask "does any campus give this right?", and the handler narrows to the campus.** The fail-closed
rule holds: no role with the right anywhere means 403 before the handler runs.

### Domain

- `RolePermission`: add `CampusId` (required, int) and `AllCampuses` (bool); add `CanViewHistory` (needs View).
  `Create` and `Set` take the campus and the flag. Unique index (SchoolRoleId, ModuleId, CampusId).
- `AccessRight`: add `ViewHistory = 7` (never reuse numbers). Update the lookup in `EnumLookupTables`.
- Built-in roles (`BuiltInRoles`) are seeded with `AllCampuses = true` on the school's main campus.

### Application interfaces

```csharp
public interface IRoleRights
{
    // Request rules use: any campus gives the right (fail closed).
    Task<bool> HasRightAsync(string userId, int schoolId, ModuleCode module, AccessRight right, CancellationToken ct);

    // Handler use: the campuses where the right holds. null = every campus of the school.
    Task<IReadOnlyCollection<int>?> GetRightCampusIdsAsync(string userId, int schoolId, ModuleCode module, AccessRight right, CancellationToken ct);
}

public interface ICampusAccess
{
    Task<IReadOnlyCollection<int>?> GetAllowedCampusIdsAsync(CancellationToken ct);          // unchanged: UserCampus
    Task EnsureCampusAsync(int? campusId, CancellationToken ct);                             // unchanged
    Task EnsureCampusAsync(int? campusId, ModuleCode module, AccessRight right, CancellationToken ct); // new: UserCampus AND role-campus
    Task<IReadOnlyCollection<int>?> GetAllowedCampusIdsAsync(ModuleCode module, AccessRight right, CancellationToken ct); // new: intersection
}
```

- `RoleRights` unions a user's roles: a row counts when `AllCampuses` or its `CampusId` is asked for. One query, cached
  per request like `CampusAccess`. Administrators and the master tenant stay unrestricted (`null`).
- Intersection rule: allowed = `UserCampus` set intersect role-campus set; `null` only when both are `null`.
- New query `GetMyRights` (module `None`, any signed-in user): returns
  `{ module, rights[], allCampuses, campusIds[] }` for modules the school has now (uses `IModuleAccess.GetEnabledCodesAsync`).
  Administrators get every right on every enabled module. Campus ids go out as `Guid` public ids (ADR-008).
- Endpoint: `GET /api/Account/me/rights` in the existing account endpoint file (thin, sends the query).
- Existing use cases move to the new `EnsureCampusAsync(campusId, module, right)` overload one by one, each with its denial
  test (ADR-018 section 4 already says moving is incremental). Phase 1 moves the role, campus and people use cases only.

### Infrastructure

- Configuration: `CampusId` FK to `Campuses`, required; check that `CanViewHistory` implies `CanView`.
- Migration `RolePermissionCampus`: add `CampusId` nullable, fill with the school's main campus (`Kind = Main`),
  set `AllCampuses = true` for old rows, then make it required. Generated with `dotnet ef`, never hand-merged.
- `ChangeAuditInterceptor` is not touched in phase 1.

### Angular

- `core/rights/rights.service.ts`: signal loaded after sign-in from `me/rights`; `can(module, right, campusId?)`.
- `navigation.ts`: `access: 'school-administrator'` becomes `right: { module, right }`; `visibleNavSections` asks
  `RightsService`. The three access kinds for master stay.
- `auth.guards.ts`: `rightsGuard(module, right)` next to `moduleGuard`; redirects to `/` when missing.
- `campus-scope.service` shows the campus filter to anyone with more than one allowed campus, not only administrators.

## Phase 2: Tenant level

Needs ADR-021 and your review before code. Questions the ADR answers: does `Subscription` and `TenantModule` move to
`Tenant` (customer) or stay on `School`; where the domain lives; the migration (one tenant per existing school).
Interfaces (draft): `Tenant` entity (`Name`, `IsMaster`, schools), `School.TenantId`, `ICurrentTenant` gains `TenantId`,
use case `AddSchoolToTenant` (`MasterTenant = true`), architecture test `EveryUseCaseDeclaresAModule`.

## Phase 3: users and roles screen

- `SchoolRole` becomes `BaseSoftDeletableEntity` (query filter combined with the tenant filter, never a second
  `HasQueryFilter`, SEC-AUTHZ-09); `DeleteRole` marks deleted.
- New command `SavePersonWithRolesAndCampuses` (one `ExecuteInTransactionAsync`), reusing `SetPersonCampuses` and the
  account role assignment logic.
- Angular: `features/people-roles` page with the users table and role editor; rights matrix component with campus
  selector and an "All campuses" choice; shared `EntityDialogComponent` for the small-entity rule; confirm delete via
  `ConfirmDialogComponent`.

## Phase 4: readable change history

- Add `SchoolRole`, `RolePermission`, `Campus`, `UserDetails` to `ChangeAuditInterceptor.AuditedTypes`; entries keep
  `ActorUserId`; the reader joins the actor's display name.
- `GetChangeAudit` takes paging and filters (entity, user, date, module) and needs `ViewHistory` per module and campus.
  A server mapper `ChangeAuditSentence` builds localised text with `IStringLocalizer` (en, ar, ur resx).
- Personal data: sentences carry names the viewer may already see; no phone, address or email values in text.

## Phase 5: calendar and My children

- `features/academic-calendar/month-grid.component` (Material + `--mm-*`, logical CSS), week view later in the same PR
  only if small; `mat-calendar` stays for date pickers.
- `navigation.ts`: `my-children` gets `access: 'parent'`, using the signed-in user's type.

## Cross-cutting decisions

- No new packages. Public ids as `Guid`, enums by name, numbers in the database.
- Tests per use case: other school, other campus, module off (functional); unit tests per new class.
- Docs in the same PRs: `AGENTS.md` section 2, README diagrams, ADR-012/016/018 amendments, ADR-021.
