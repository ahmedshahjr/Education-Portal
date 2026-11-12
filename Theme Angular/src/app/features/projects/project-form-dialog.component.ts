import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { FormInputComponent } from '../../shared/components/form-input/form-input.component';
import { ModalComponent } from '../../shared/components/modal/modal.component';
import { SelectInputComponent, SelectOption } from '../../shared/components/select-input/select-input.component';
import { ProjectCategory, ProjectItem } from '../../shared/models/dashboard.models';

export interface ProjectDialogData {
  readonly project?: ProjectItem;
}

@Component({
  selector: 'app-project-form-dialog',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    MatButtonModule,
    ModalComponent,
    FormInputComponent,
    SelectInputComponent,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <app-modal
      [title]="data.project ? 'Edit Project' : 'Create Project'"
      subtitle="Update workspace project metadata and budget allocation"
      (closed)="dialogRef.close()"
    >
      <form [formGroup]="form" (ngSubmit)="submit()" class="mm-project-form" id="projectForm">
        <app-form-input
          label="Project Name"
          placeholder="e.g. MaterialM Analytics Portal"
          [control]="form.controls.name"
        />

        <div class="mm-form-pair">
          <app-form-input
            label="Budget (USD)"
            type="number"
            placeholder="15000"
            [control]="form.controls.budget"
          />
          <app-form-input
            label="Project Leader"
            placeholder="e.g. Erin"
            [control]="form.controls.leader"
          />
        </div>

        <div class="mm-form-pair">
          <app-select-input
            label="Category"
            [control]="form.controls.category"
            [options]="categoryOptions"
          />
          <app-select-input
            label="Status"
            [control]="form.controls.status"
            [options]="statusOptions"
          />
        </div>
      </form>

      <div modal-actions>
        <button mat-stroked-button type="button" (click)="dialogRef.close()">Cancel</button>
        <button mat-flat-button color="primary" type="submit" form="projectForm">
          {{ data.project ? 'Save Changes' : 'Create Project' }}
        </button>
      </div>
    </app-modal>
  `,
  styles: `
    .mm-project-form {
      display: flex;
      flex-direction: column;
      gap: 12px;
      min-width: 420px;
      max-width: 100%;
    }

    .mm-form-pair {
      display: grid;
      grid-template-columns: repeat(2, minmax(0, 1fr));
      gap: 14px;
    }

    @media (max-width: 560px) {
      .mm-project-form {
        min-width: 0;
      }

      .mm-form-pair {
        grid-template-columns: 1fr;
      }
    }
  `,
})
export class ProjectFormDialogComponent {
  protected readonly data = inject<ProjectDialogData>(MAT_DIALOG_DATA);
  protected readonly dialogRef = inject(MatDialogRef<ProjectFormDialogComponent>);
  private readonly fb = inject(FormBuilder);

  protected readonly categoryOptions: readonly SelectOption<ProjectCategory>[] = [
    { value: 'SaaS', label: 'SaaS' },
    { value: 'Mobile', label: 'Mobile' },
    { value: 'Others', label: 'Others' },
  ];

  protected readonly statusOptions: readonly SelectOption<ProjectItem['status']>[] = [
    { value: 'Active', label: 'Active' },
    { value: 'In Review', label: 'In Review' },
    { value: 'Completed', label: 'Completed' },
  ];

  protected readonly form = this.fb.nonNullable.group({
    name: [this.data.project?.name ?? '', [Validators.required, Validators.minLength(2)]],
    budget: [this.data.project?.budget ?? 4500, [Validators.required, Validators.min(0)]],
    leader: [this.data.project?.leader ?? '', [Validators.required]],
    category: [this.data.project?.category ?? ('SaaS' as ProjectCategory), [Validators.required]],
    status: [this.data.project?.status ?? ('Active' as ProjectItem['status']), [Validators.required]],
  });

  protected submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    this.dialogRef.close(this.form.getRawValue());
  }
}
