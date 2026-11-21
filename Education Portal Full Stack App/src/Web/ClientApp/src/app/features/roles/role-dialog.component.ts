import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { FormBuilder, FormControl, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { ModuleTextPipe } from '../../core/i18n/module-text.pipe';
import { ModuleDto, RolePermissionDto, SchoolRoleDto } from '../../web-api-client';
import { RIGHT_KEYS, Right, rolePermissions } from './role-rights';

/** What the role dialog is given: the role being changed (null when adding) and the modules the school has. */
export interface RoleDialogData {
  readonly role: SchoolRoleDto | null;
  readonly modules: readonly ModuleDto[];
}

/** What the dialog returns on save: the role's name, description and rights. */
export interface RoleDialogResult {
  readonly name: string;
  readonly description: string | undefined;
  readonly permissions: RolePermissionDto[];
}

/**
 * Adds a role or changes one, in a dialog (ADR-024: small entities get a dialog). The rights are a grid with one row per
 * module and one column per right. A built-in role keeps its name, so the name field is locked for it.
 */
@Component({
  selector: 'app-role-dialog',
  imports: [ReactiveFormsModule, MatDialogModule, MatButtonModule, MatFormFieldModule, MatInputModule, ModuleTextPipe],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './role-dialog.component.html',
  styleUrl: './role-dialog.component.css',
})
export class RoleDialogComponent {
  private readonly data = inject<RoleDialogData>(MAT_DIALOG_DATA);
  private readonly dialogRef = inject<MatDialogRef<RoleDialogComponent, RoleDialogResult>>(MatDialogRef);
  private readonly formBuilder = inject(FormBuilder);

  protected readonly role = this.data.role;
  protected readonly modules = this.data.modules;

  private readonly rightLabels: Record<Right, string> = {
    canView: $localize`:@@roles.right.view:View`,
    canCreate: $localize`:@@roles.right.create:Create`,
    canEdit: $localize`:@@roles.right.edit:Edit`,
    canDelete: $localize`:@@roles.right.delete:Delete`,
    canApprove: $localize`:@@roles.right.approve:Approve`,
    canExport: $localize`:@@roles.right.export:Export`,
  };

  protected readonly rights: readonly { key: Right; label: string }[] = RIGHT_KEYS.map((key) => ({ key, label: this.rightLabels[key] }));

  /** One checkbox per (module, right). */
  private readonly grants = new Map<string, FormControl<boolean>>();

  protected readonly form = this.formBuilder.nonNullable.group({
    name: [this.role?.name ?? '', [Validators.required, Validators.maxLength(100)]],
    description: [this.role?.description ?? '', [Validators.maxLength(500)]],
  });

  constructor() {
    if (this.role?.isSystem) {
      this.form.controls.name.disable();
    }
    for (const module of this.modules) {
      const permission = this.role?.permissions.find((p) => p.module === module.code);
      for (const right of RIGHT_KEYS) {
        this.grant(module.code, right).setValue(permission?.[right] ?? false);
      }
    }
  }

  protected grant(module: string, right: Right): FormControl<boolean> {
    const key = `${module}:${right}`;
    let control = this.grants.get(key);
    if (!control) {
      control = this.formBuilder.nonNullable.control(false);
      this.grants.set(key, control);
    }
    return control;
  }

  protected save(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const value = this.form.getRawValue();
    const permissions = rolePermissions(
      this.modules.map((m) => m.code),
      (module, right) => this.grant(module, right).value
    ).map((p) => new RolePermissionDto(p));

    this.dialogRef.close({ name: value.name.trim(), description: value.description.trim() || undefined, permissions });
  }
}
