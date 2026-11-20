import { describe, expect, it } from 'vitest';
import { ModuleCode } from '../../web-api-client';
import { rolePermissions } from './role-rights';

describe('rolePermissions', () => {
  it('sends only modules with a right ticked', () => {
    const ticked = (module: ModuleCode, right: string) => module === ModuleCode.Students && right === 'canView';

    expect(rolePermissions([ModuleCode.Students, ModuleCode.Staff], ticked)).toEqual([
      { module: ModuleCode.Students, canView: true, canCreate: false, canEdit: false, canDelete: false, canApprove: false, canExport: false },
    ]);
  });

  it('adds View to a module where only another right is ticked', () => {
    const ticked = (_: ModuleCode, right: string) => right === 'canApprove' || right === 'canExport';

    const [permission] = rolePermissions([ModuleCode.Admissions], ticked);

    expect(permission).toEqual({
      module: ModuleCode.Admissions,
      canView: true,
      canCreate: false,
      canEdit: false,
      canDelete: false,
      canApprove: true,
      canExport: true,
    });
  });

  it('sends nothing when no right is ticked', () => {
    expect(rolePermissions([ModuleCode.Students], () => false)).toEqual([]);
  });
});
