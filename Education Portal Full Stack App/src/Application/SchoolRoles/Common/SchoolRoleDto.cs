namespace EducationPortal.Application.SchoolRoles.Common;

/// <summary>A school's role with the rights it gives in the modules the school has now.</summary>
public record SchoolRoleDto(Guid Id, string Name, IReadOnlyList<RolePermissionDto> Permissions);

/// <summary>What a role may do in one module, addressed by the module's code.</summary>
public record RolePermissionDto(string Module, bool CanView, bool CanCreate, bool CanEdit, bool CanDelete);
