namespace EducationPortal.Application.SchoolRoles.Common;

/// <summary>A school's role with the rights it gives in the modules the school has now.</summary>
/// <remarks>A system role (<paramref name="IsSystem"/>) is built in: it keeps its name and cannot be deleted.</remarks>
public record SchoolRoleDto(Guid Id, string Name, string? Description, bool IsSystem, IReadOnlyList<RolePermissionDto> Permissions);

/// <summary>
/// What a role may do in one module, addressed by the module's code, and on which campuses (ADR-022). When
/// <paramref name="AllCampuses"/> is true the rights apply to every campus of the school and <paramref name="CampusIds"/>
/// is ignored; otherwise they apply to the campuses named by their public ids, which must be at least one when the module
/// has a right. The defaults keep the all-campuses behaviour for callers that name no campus.
/// </summary>
public record RolePermissionDto(
    ModuleCode Module,
    bool CanView,
    bool CanCreate,
    bool CanEdit,
    bool CanDelete,
    bool CanApprove,
    bool CanExport,
    bool CanViewHistory = false,
    bool AllCampuses = true,
    IReadOnlyList<Guid>? CampusIds = null);
