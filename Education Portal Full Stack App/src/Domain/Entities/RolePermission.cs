namespace EducationPortal.Domain.Entities;

/// <summary>
/// What a role may do in one module: view, create, edit and delete. Creating, editing and deleting need viewing. Owned
/// by the school (<c>SchoolId</c>) and covered by the tenant query filter (SEC-AUTHZ-03). The row is kept when the
/// school's subscription stops giving the module, so the rights come back with the module; while it is off they are
/// neither shown nor used.
/// </summary>
public class RolePermission : BaseAuditableEntity
{
    private RolePermission()
    {
        // For EF Core.
    }

    private RolePermission(SchoolRole role, PortalModule module, bool canView, bool canCreate, bool canEdit, bool canDelete)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(role.Id);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(module.Id);

        SchoolId = role.SchoolId;
        SchoolRoleId = role.Id;
        ModuleId = module.Id;
        Set(canView, canCreate, canEdit, canDelete);
    }

    public static RolePermission Create(SchoolRole role, PortalModule module, bool canView, bool canCreate, bool canEdit, bool canDelete)
    {
        ArgumentNullException.ThrowIfNull(role);
        ArgumentNullException.ThrowIfNull(module);

        return new RolePermission(role, module, canView, canCreate, canEdit, canDelete);
    }

    public int SchoolId { get; private set; }

    public int SchoolRoleId { get; private set; }

    public int ModuleId { get; private set; }

    public bool CanView { get; private set; }

    public bool CanCreate { get; private set; }

    public bool CanEdit { get; private set; }

    public bool CanDelete { get; private set; }

    public void Set(bool canView, bool canCreate, bool canEdit, bool canDelete)
    {
        if (!canView && (canCreate || canEdit || canDelete))
        {
            throw new ArgumentException("Creating, editing and deleting need the right to view.");
        }

        CanView = canView;
        CanCreate = canCreate;
        CanEdit = canEdit;
        CanDelete = canDelete;
    }

    /// <summary>True when the role has no right at all in the module, so the row is not needed.</summary>
    public bool IsEmpty => !CanView;
}
