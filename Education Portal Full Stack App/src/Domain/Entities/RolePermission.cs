namespace EducationPortal.Domain.Entities;

/// <summary>
/// What a role may do in one module: view, create, edit, delete, approve and export. Every right but viewing needs viewing. Owned
/// by the school (<c>SchoolId</c>) and covered by the tenant query filter (SEC-AUTHZ-03). The row is kept when the
/// school's subscription stops giving the module, so the rights come back with the module; while it is off they are
/// neither shown nor used.
/// </summary>
public class RolePermission : BaseAuditableEntity, ITenantOwned
{
    private RolePermission()
    {
        // For EF Core.
    }

    private RolePermission(SchoolRole role, PortalModule module, bool canView, bool canCreate, bool canEdit, bool canDelete, bool canApprove, bool canExport)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(role.Id);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(module.Id);

        SchoolId = role.SchoolId;
        SchoolRoleId = role.Id;
        ModuleId = module.Id;
        Set(canView, canCreate, canEdit, canDelete, canApprove, canExport);
    }

    public static RolePermission Create(
        SchoolRole role, PortalModule module, bool canView, bool canCreate, bool canEdit, bool canDelete, bool canApprove, bool canExport)
    {
        ArgumentNullException.ThrowIfNull(role);
        ArgumentNullException.ThrowIfNull(module);

        return new RolePermission(role, module, canView, canCreate, canEdit, canDelete, canApprove, canExport);
    }

    public int SchoolId { get; private set; }

    public int SchoolRoleId { get; private set; }

    public int ModuleId { get; private set; }

    public bool CanView { get; private set; }

    public bool CanCreate { get; private set; }

    public bool CanEdit { get; private set; }

    public bool CanDelete { get; private set; }

    /// <summary>May approve what waits for a decision in the module (for example an admission or a leave request).</summary>
    public bool CanApprove { get; private set; }

    /// <summary>May download the module's lists as a file.</summary>
    public bool CanExport { get; private set; }

    public void Set(bool canView, bool canCreate, bool canEdit, bool canDelete, bool canApprove, bool canExport)
    {
        if (!canView && (canCreate || canEdit || canDelete || canApprove || canExport))
        {
            throw new ArgumentException("Creating, editing, deleting, approving and exporting need the right to view.");
        }

        CanView = canView;
        CanCreate = canCreate;
        CanEdit = canEdit;
        CanDelete = canDelete;
        CanApprove = canApprove;
        CanExport = canExport;
    }

    /// <summary>True when the role has no right at all in the module, so the row is not needed.</summary>
    public bool IsEmpty => !CanView;
}
