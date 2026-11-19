import { describe, expect, it } from 'vitest';
import { SchoolRoleDto, RolePermissionDto } from '../../web-api-client';
import { modulesWithRights, NO_RIGHTS, orderedModuleCodes, rightsOf, toPermissions, withRight } from './role-rights';

const role = (permissions: RolePermissionDto[]): SchoolRoleDto =>
  new SchoolRoleDto({ id: 'role-1', name: 'Teacher', permissions });

describe('role rights', () => {
  it('lists only the modules the school has, in the portal order', () => {
    const enabled = new Set(['FEES', 'STUDENTS', 'SCHOOL']);

    expect(orderedModuleCodes(enabled)).toEqual(['SCHOOL', 'STUDENTS', 'FEES']);
  });

  it('gives no rights in a module the role does not mention', () => {
    const rights = rightsOf(role([]), ['STUDENTS']);

    expect(rights['STUDENTS']).toEqual(NO_RIGHTS);
  });

  it('reads the rights the role gives', () => {
    const rights = rightsOf(
      role([new RolePermissionDto({ module: 'STUDENTS', canView: true, canCreate: false, canEdit: true, canDelete: false })]),
      ['STUDENTS']
    );

    expect(rights['STUDENTS']).toEqual({ canView: true, canCreate: false, canEdit: true, canDelete: false });
  });

  it('turns view on with any other right', () => {
    const rights = withRight(NO_RIGHTS, 'canDelete', true);

    expect(rights).toEqual({ canView: true, canCreate: false, canEdit: false, canDelete: true });
  });

  it('turns every other right off when view is turned off', () => {
    const rights = withRight({ canView: true, canCreate: true, canEdit: true, canDelete: true }, 'canView', false);

    expect(rights).toEqual(NO_RIGHTS);
  });

  it('turns off one right without changing the others', () => {
    const rights = withRight({ canView: true, canCreate: true, canEdit: true, canDelete: true }, 'canEdit', false);

    expect(rights).toEqual({ canView: true, canCreate: true, canEdit: false, canDelete: true });
  });

  it('does not change the rights it was given', () => {
    const before = { ...NO_RIGHTS };

    withRight(before, 'canCreate', true);

    expect(before).toEqual(NO_RIGHTS);
  });

  it('sends one row per module, in order, with missing rights as false', () => {
    const permissions = toPermissions({ STUDENTS: { canView: true, canCreate: false, canEdit: false, canDelete: false } }, ['STUDENTS', 'STAFF']);

    expect(permissions.map((p) => p.module)).toEqual(['STUDENTS', 'STAFF']);
    expect(permissions[1]).toMatchObject({ canView: false, canCreate: false, canEdit: false, canDelete: false });
  });

  it('counts the modules a role gives any right in', () => {
    const count = modulesWithRights(
      role([
        new RolePermissionDto({ module: 'STUDENTS', canView: true, canCreate: false, canEdit: false, canDelete: false }),
        new RolePermissionDto({ module: 'STAFF', canView: false, canCreate: false, canEdit: false, canDelete: false }),
      ])
    );

    expect(count).toBe(1);
  });
});
