import { TestBed } from '@angular/core/testing';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { describe, expect, it, vi } from 'vitest';
import { ModuleCode, ModuleDto, SchoolRoleDto } from '../../web-api-client';
import { RoleDialogComponent } from './role-dialog.component';

const teacher = {
  id: '0190f3a0-0000-7000-8000-000000000001',
  name: 'Teacher',
  description: 'Teaches classes',
  isSystem: true,
  permissions: [{ module: 'Students', canView: true, canCreate: true, canEdit: false, canDelete: false, canApprove: true, canExport: false }],
} as unknown as SchoolRoleDto;

const students = { code: ModuleCode.Students, name: 'Students', description: '', isCore: true, isActive: true, sortOrder: 1 } as unknown as ModuleDto;

function open(role: SchoolRoleDto | null) {
  const close = vi.fn();
  TestBed.configureTestingModule({
    imports: [RoleDialogComponent],
    providers: [
      { provide: MAT_DIALOG_DATA, useValue: { role, modules: [students] } },
      { provide: MatDialogRef, useValue: { close } },
    ],
  });
  const fixture = TestBed.createComponent(RoleDialogComponent);
  fixture.detectChanges();
  return { close, dialog: fixture.componentInstance, fixture };
}

describe('RoleDialogComponent', () => {
  it('locks the name of a built-in role and shows its rights', () => {
    const { dialog } = open(teacher);

    expect(dialog['form'].controls.name.disabled).toBe(true);
    expect(dialog['grant']('Students', 'canApprove').value).toBe(true);
    expect(dialog['grant']('Students', 'canExport').value).toBe(false);
  });

  it('closes with the name, description and rights as they were changed', () => {
    const { close, dialog } = open(teacher);

    dialog['form'].controls.description.setValue('Teaches and marks');
    dialog['grant']('Students', 'canExport').setValue(true);
    dialog['save']();

    expect(close).toHaveBeenCalledWith({
      name: 'Teacher',
      description: 'Teaches and marks',
      permissions: [expect.objectContaining({ module: 'Students', canView: true, canApprove: true, canExport: true })],
    });
  });

  it('does not close while the name is empty when adding a role', () => {
    const { close, dialog } = open(null);

    dialog['save']();

    expect(close).not.toHaveBeenCalled();
    expect(dialog['form'].controls.name.invalid).toBe(true);
  });
});
