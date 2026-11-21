namespace EducationPortal.Application.SchoolRoles.Common;

/// <summary>A school's role with the rights it gives in the modules the school has now.</summary>
/// <remarks>A system role (<paramref name="IsSystem"/>) is built in: it keeps its name and cannot be deleted.</remarks>
public record SchoolRoleDto(Guid Id, string Name, string? Description, bool IsSystem, IReadOnlyList<RolePermissionDto> Permissions);

/// <summary>What a role may do in one module, addressed by the module's code.</summary>
public record RolePermissionDto(ModuleCode Module, bool CanView, bool CanCreate, bool CanEdit, bool CanDelete, bool CanApprove, bool CanExport, bool CanViewHistory = false);
