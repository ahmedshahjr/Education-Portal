import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { moduleText } from '../../core/i18n/module-text.pipe';
import { ModuleDto, PlanDto } from '../../web-api-client';

/** Upper-case letters, digits and underscores, starting with a letter (the server checks it again, SEC-INPUT-01). */
const PLAN_CODE_PATTERN = /^[A-Z][A-Z0-9_]*$/;

export interface PlanDialogData {
  /** The plan being changed, or null when a plan is added. */
  readonly plan: PlanDto | null;
  /** The paid modules a plan can include (core modules are in every plan). */
  readonly modules: readonly ModuleDto[];
}

/** What the dialog returns. The code is only sent when a plan is added; it cannot change later. */
export interface PlanChanges {
  readonly code: string;
  readonly name: string;
  readonly description: string;
  readonly isActive: boolean;
  readonly modules: readonly string[];
}

/** Adds a plan or changes its name, description, switch and paid modules. The server checks everything again. */
@Component({
  selector: 'app-plan-dialog',
  imports: [ReactiveFormsModule, MatDialogModule, MatButtonModule, MatCheckboxModule, MatFormFieldModule, MatInputModule, MatSlideToggleModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './plan-dialog.component.html',
  styleUrl: './plan-dialog.component.css',
})
export class PlanDialogComponent {
  private readonly data = inject<PlanDialogData>(MAT_DIALOG_DATA);
  private readonly dialogRef = inject<MatDialogRef<PlanDialogComponent, PlanChanges>>(MatDialogRef);

  protected readonly plan = this.data.plan;
  protected readonly modules = this.data.modules;

  protected readonly form = inject(FormBuilder).nonNullable.group({
    code: [
      { value: this.plan?.code ?? '', disabled: this.plan !== null },
      [Validators.required, Validators.maxLength(50), Validators.pattern(PLAN_CODE_PATTERN)],
    ],
    name: [this.plan?.name ?? '', [Validators.required, Validators.maxLength(100)]],
    description: [this.plan?.description ?? '', [Validators.maxLength(500)]],
    isActive: [this.plan?.isActive ?? true],
  });

  protected readonly selected = signal<ReadonlySet<string>>(new Set(this.plan?.modules ?? []));

  protected moduleName(module: ModuleDto): string {
    return moduleText(module);
  }

  protected toggle(code: string, checked: boolean): void {
    const next = new Set(this.selected());
    if (checked) {
      next.add(code);
    } else {
      next.delete(code);
    }
    this.selected.set(next);
  }

  protected save(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const value = this.form.getRawValue();
    this.dialogRef.close({
      code: value.code.trim(),
      name: value.name.trim(),
      description: value.description.trim(),
      isActive: value.isActive,
      modules: this.modules.map((m) => m.code).filter((code) => this.selected().has(code)),
    });
  }
}
