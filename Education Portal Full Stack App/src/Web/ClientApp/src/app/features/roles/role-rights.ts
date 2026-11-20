import { IRolePermissionDto, ModuleCode } from '../../web-api-client';

/** The six rights a role gives per module, in the order the table shows them. */
export const RIGHT_KEYS = ['canView', 'canCreate', 'canEdit', 'canDelete', 'canApprove', 'canExport'] as const;

export type Right = (typeof RIGHT_KEYS)[number];

/**
 * The rights to send for a role: one entry per module that has at least one right ticked. Every right needs View, so a
 * tick on any other right sends View with it, as the server requires.
 */
export function rolePermissions(modules: readonly ModuleCode[], ticked: (module: ModuleCode, right: Right) => boolean): IRolePermissionDto[] {
  return modules
    .map((module) => {
      const rights = Object.fromEntries(RIGHT_KEYS.map((right) => [right, ticked(module, right)])) as Record<Right, boolean>;
      const any = RIGHT_KEYS.some((right) => rights[right]);
      return { module, ...rights, canView: any };
    })
    .filter((p) => p.canView);
}
