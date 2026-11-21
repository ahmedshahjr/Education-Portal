namespace EducationPortal.Domain.Entities;

/// <summary>
/// What a role may do in one module on one campus: view, create, edit, delete, approve, export and read the change
/// history. Every right but viewing needs viewing. When <see cref="AllCampuses"/> is true the rights apply to every campus
/// of the school, including campuses added later, and <see cref="CampusId"/> only records where the row was made (the
/// main campus); otherwise they apply to <see cref="CampusId"/> only (ADR-022). Owned by the school
/// (<c>SchoolId</c>) and covered by the tenant query filter (SEC-AUTHZ-03). The row is kept when the school's
/// subscription stops giving the module, so the rights come back with the module; while it is off they are neither
/// shown nor used.
/// </summary>
public class RolePermission : BaseAuditableEntity, ITenantOwned
{
    private RolePermission()
    {
        // For EF Core.
    }

    private RolePermission(
        SchoolRole role, PortalModule module, Campus campus, bool allCampuses,
        bool canView, bool canCreate, bool canEdit, bool canDelete, bool canApprove, bool canExport, bool canViewHistory)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(role.Id);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(module.Id);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(campus.Id);

        if (campus.SchoolId != role.SchoolId)
        {
            throw new ArgumentException("The campus must belong to the role's school.", nameof(campus));
        }

        SchoolId = role.SchoolId;
        SchoolRoleId = role.Id;
        ModuleId = module.Id;
        CampusId = campus.Id;
        AllCampuses = allCampuses;
        Set(canView, canCreate, canEdit, canDelete, canApprove, canExport, canViewHistory);
    }

    public static RolePermission Create(
        SchoolRole role, PortalModule module, Campus campus, bool allCampuses,
        bool canView, bool canCreate, bool canEdit, bool canDelete, bool canApprove, bool canExport, bool canViewHistory)
    {
        ArgumentNullException.ThrowIfNull(role);
        ArgumentNullException.ThrowIfNull(module);
        ArgumentNullException.ThrowIfNull(campus);

        return new RolePermission(role, module, campus, allCampuses, canView, canCreate, canEdit, canDelete, canApprove, canExport, canViewHistory);
    }

    public int SchoolId { get; private set; }

    public int SchoolRoleId { get; private set; }

    public int ModuleId { get; private set; }

    /// <summary>The campus the rights are for; with <see cref="AllCampuses"/> it is only where the row was made.</summary>
    public int CampusId { get; private set; }

    /// <summary>True when the rights apply to every campus of the school, including ones added later.</summary>
    public bool AllCampuses { get; private set; }

    public bool CanView { get; private set; }

    public bool CanCreate { get; private set; }

    public bool CanEdit { get; private set; }

    public bool CanDelete { get; private set; }

    /// <summary>May approve what waits for a decision in the module (for example an admission or a leave request).</summary>
    public bool CanApprove { get; private set; }

    /// <summary>May download the module's lists as a file.</summary>
    public bool CanExport { get; private set; }

    /// <summary>May read the change history of the module's records (ADR-023).</summary>
    public bool CanViewHistory { get; private set; }

    public void Set(bool canView, bool canCreate, bool canEdit, bool canDelete, bool canApprove, bool canExport, bool canViewHistory)
    {
        if (!canView && (canCreate || canEdit || canDelete || canApprove || canExport || canViewHistory))
        {
            throw new ArgumentException("Creating, editing, deleting, approving, exporting and reading the history need the right to view.");
        }

        CanView = canView;
        CanCreate = canCreate;
        CanEdit = canEdit;
        CanDelete = canDelete;
        CanApprove = canApprove;
        CanExport = canExport;
        CanViewHistory = canViewHistory;
    }

    /// <summary>Makes the rights apply to every campus of the school (ADR-022). The row keeps the campus it was made on.</summary>
    public void ApplyToAllCampuses() => AllCampuses = true;

    /// <summary>True when the role has no right at all in the module, so the row is not needed.</summary>
    public bool IsEmpty => !CanView;
}
