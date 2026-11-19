import { MODULE_CODES } from '../../core/modules/modules.service';
import { RolePermissionDto, SchoolRoleDto } from '../../web-api-client';

/** The four rights a role can give in one module. Creating, editing and deleting need the right to view (the server checks it). */
export type RightName = 'canView' | 'canCreate' | 'canEdit' | 'canDelete';

export type RightFlags = Record<RightName, boolean>;

export const NO_RIGHTS: RightFlags = { canView: false, canCreate: false, canEdit: false, canDelete: false };

/** The modules the school has, in the portal's module order (MODULE_CODES). Modules it does not have are not shown. */
export function orderedModuleCodes(enabled: ReadonlySet<string>): string[] {
  return Object.values(MODULE_CODES).filter((code) => enabled.has(code));
}

/** The rights of a role in each of the given modules; a module the role does not mention gives no rights. */
export function rightsOf(role: SchoolRoleDto | null, moduleCodes: readonly string[]): Record<string, RightFlags> {
  const byModule = new Map((role?.permissions ?? []).map((p) => [p.module, p] as const));
  return Object.fromEntries(
    moduleCodes.map((code) => {
      const permission = byModule.get(code);
      return [
        code,
        permission
          ? { canView: permission.canView, canCreate: permission.canCreate, canEdit: permission.canEdit, canDelete: permission.canDelete }
          : { ...NO_RIGHTS },
      ];
    })
  );
}

/**
 * Sets one right. Turning on create, edit or delete also turns on view, and turning off view turns the others off, as the
 * server requires. Returns a new object: the page's state is never changed in place.
 */
export function withRight(rights: RightFlags, name: RightName, value: boolean): RightFlags {
  if (name === 'canView') {
    return value ? { ...rights, canView: true } : { ...NO_RIGHTS };
  }

  return value ? { ...rights, canView: true, [name]: true } : { ...rights, [name]: false };
}

/** The permissions to send: one row per module the school has, in module order. */
export function toPermissions(rights: Record<string, RightFlags>, moduleCodes: readonly string[]): RolePermissionDto[] {
  return moduleCodes.map(
    (code) =>
      new RolePermissionDto({
        module: code,
        canView: rights[code]?.canView ?? false,
        canCreate: rights[code]?.canCreate ?? false,
        canEdit: rights[code]?.canEdit ?? false,
        canDelete: rights[code]?.canDelete ?? false,
      })
  );
}

/** How many modules a role gives any right in. */
export function modulesWithRights(role: SchoolRoleDto): number {
  return role.permissions.filter((p) => p.canView || p.canCreate || p.canEdit || p.canDelete).length;
}
