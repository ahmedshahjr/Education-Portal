# Pseudocode: core logic per requirement

Companion to the spec (R-ids) and the design. Pseudocode only; names match the design.

## Phase 1: rights

```
R1.1  RolePermission.Create(role, module, campus, allCampuses, rights...)
        require campus.SchoolId == role.SchoolId           // never null, same school
        require rights obey: any right other than View needs View; ViewHistory needs View
        store CampusId = campus.Id, AllCampuses = allCampuses
      index unique (SchoolRoleId, ModuleId, CampusId)       // duplicate -> unique violation
      migration: add CampusId nullable
                 for each old row: CampusId = main campus of row.SchoolId; AllCampuses = true
                 set CampusId NOT NULL

R1.2  Set(...): if ViewHistory and not View -> throw ArgumentException
      AccessRight.ViewHistory = 7; add lookup row; add CanViewHistory column (default false)

R1.3  behaviour.Handle(request):
        for each attribute with Right != None (module required, else InvalidOperationException)
          skip if master tenant or Administrator
          if not IRoleRights.HasRightAsync(user, school, module, right): 403     // any campus
      handler (campus-scoped use case):
        ICampusAccess.EnsureCampusAsync(campusId, module, right)
      RoleRights.GetRightCampusIdsAsync(user, school, module, right):
        roleIds   = AccountRoles of user in school
        rows      = RolePermissions where role in roleIds and module matches and right flag true
        if any row.AllCampuses -> return null                 // every campus
        return distinct rows.CampusId                          // union across roles

R1.4  ICampusAccess.GetAllowedCampusIdsAsync(module, right):
        a = existing UserCampus set (null if admin/master)
        b = RoleRights.GetRightCampusIdsAsync(...)  (null if admin/master)
        if a is null return b; if b is null return a
        return a INTERSECT b
      EnsureCampusAsync(campusId, module, right):
        if campusId is null: return                           // whole-school row, role rights decide
        allowed = GetAllowedCampusIds(module, right)
        if allowed != null and campusId not in allowed: 403

R1.5  GetMyRights():
        enabled = IModuleAccess.GetEnabledCodesAsync(school)
        if Administrator: every enabled module, all rights, allCampuses = true
        else for each enabled module: rights = flags from the user's rows;
             allCampuses = any row.AllCampuses; campusIds = distinct campus public ids
        return list; modules not enabled never appear

R1.6  Angular RightsService.load() after sign-in -> signal
      can(module, right, campusId?) : admin -> true; entry.allCampuses or campusId in entry.campusIds
      visibleNavSections: item shown if item.right is null or can(item.module, item.right)
      rightsGuard(module, right): can(...) ? true : redirect '/'

R1.7  no change; existing administrator tests must stay green
```

## Phase 2: tenant

```
R2.1  migration: create Tenants; for each School: insert Tenant(name = School.Name, IsMaster = School.IsMaster);
      School.TenantId = that tenant; move Subscription/TenantModule/Invoice SchoolId -> TenantId via school's tenant
R2.2  unique filtered index on Tenants(IsMaster) where IsMaster = true
R2.3  AddSchoolToTenant(tenantId, name, ...) [MasterTenant]: behaviour refuses when not on master host (403);
      find tenant via filtered query (404 if absent); School.Create adds main campus; save in one transaction
R2.4  Register(...): ExecuteInTransactionAsync { tenant = Tenant.Create; school = School.Create(first school);
        admin = create Administrator account; grant default modules to tenant (SchoolDefaultModules); trial }
R2.5  architecture test: for every IRequest type not [AllowAnonymous]: assert it has [Authorize] with Module != None,
      or MasterTenant = true, or is in the exemption list; list failing types
R2.6  Angular route config: add moduleGuard/rightsGuard to subscription, audit, branding, admissions
R2.7  both registration paths create an Administrator account bound to the tenant's first school; no new role
R2.8  UpdateTenant(tenantId, fields...) [MasterTenant]: resolve tenant; apply each field through domain methods;
      change-audit interceptor writes entries; password reset -> new one-time password, MustChangePassword = true
R2.9  SetSchoolDomain(schoolId, domain) [MasterTenant]; normalise and check unique; school admin call -> 403
      sign-in(host, email): if host is master host and school.HasOwnDomain -> refuse (same error as wrong password)
```

## Phase 3: users and roles

```
R3.1  SaveRole(roleId?, name, description, matrix): for each module in matrix: refuse if module not enabled for tenant;
        remove rows not in matrix; if allCampuses: one row (main campus, AllCampuses = true)
        else one row per chosen campus; role.Name unchanged when IsSystem
R3.2  SavePerson(person, roleIds, campusIds): ExecuteInTransactionAsync { validate roles/campuses belong to school;
        upsert UserDetails; replace AccountRoles; replace UserCampuses }; any failure rolls back all
R3.3  DeleteRole(id): if IsSystem -> validation error; else MarkDeleted (soft); filtered from queries; AccountRoles of a
        deleted role give no rights (rights query joins only live roles)
R3.4  Angular: entity registry { roles, campuses, holidays, events, shifts: 'dialog'; people, schools, admissions: 'page' };
        open(entity, id?) -> dialog or router.navigate; delete -> ConfirmDialog -> command
```

## Phase 4: history

```
R4.1  ChangeAuditInterceptor.AuditedTypes += SchoolRole, RolePermission, Campus, UserDetails
      for each changed property: if in the entity's personal-data set -> record { property, changed: true } without old/new
R4.2  GetChangeAudit(filters, page) [Module = entry module, Right = ViewHistory]:
        allowedCampuses = ICampusAccess.GetAllowedCampusIds(module, ViewHistory)
        query entries of the school; filter by entity, user, date range, module, campus in allowedCampuses
        order by OccurredAt desc; page
R4.3  ChangeAuditSentence.Build(entry, actorName, culture): pick resource by (entity, action, property);
        format with actor name, entity label, time in the user's time zone; en, ar, ur resx required
R4.4  Angular: menu item and route need can('<module>', 'ViewHistory'); page shows filters, paging, sentences
```

## Phase 5: calendar

```
R5.1  MonthGrid(year, month, events, holidays): weeks start per country (week start setting);
        build 6x7 cells of date keys; place items by dateKey; logical CSS; mirrors in RTL
R5.2  all DateOnly via fromApiDate/dateKey/calendarDate; never new Date(string) on a date-only value
R5.3  navigation item my-children: access 'parent' = signed-in user type is Parent
```

## Tests mapped

Each block above gets, in this order: a failing test (t6), the code (t7), then the three denial tests (other school,
other campus, module off) for every new use case.
