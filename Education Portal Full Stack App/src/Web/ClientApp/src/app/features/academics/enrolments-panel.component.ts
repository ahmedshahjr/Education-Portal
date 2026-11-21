import { ChangeDetectionStrategy, Component, inject, OnInit, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { problemMessages } from '../../core/http/problem-details';
import { ToastService } from '../../shared/services/toast.service';
import { EnrolStudentCommand, EnrolmentsClient } from '../../web-api-client';
import { AcademicsLookups } from './academics-lookups.service';
import { offeringLabel, personName } from './academics.logic';

/** Enrols a student of the school in a class offering. A student can be in one class offering of an academic year once. */
@Component({
  selector: 'app-enrolments-panel',
  imports: [ReactiveFormsModule, MatButtonModule, MatCardModule, MatFormFieldModule, MatSelectModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './enrolments-panel.component.html',
  styleUrl: './academics.css',
})
export class EnrolmentsPanelComponent implements OnInit {
  private readonly client = inject(EnrolmentsClient);
  private readonly toast = inject(ToastService);
  protected readonly lookups = inject(AcademicsLookups);

  protected readonly offeringLabel = offeringLabel;
  protected readonly personName = personName;

  protected readonly form = inject(FormBuilder).nonNullable.group({
    studentId: ['', Validators.required],
    classOfferingId: ['', Validators.required],
  });

  protected readonly busy = signal(false);
  protected readonly errors = signal<readonly string[]>([]);

  ngOnInit(): void {
    this.lookups.loadOfferings();
    this.lookups.loadStudents();
  }

  protected save(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const value = this.form.getRawValue();
    this.busy.set(true);
    this.errors.set([]);
    this.client.enrolStudent(new EnrolStudentCommand({ studentId: value.studentId, classOfferingId: value.classOfferingId })).subscribe({
      next: () => {
        this.busy.set(false);
        this.form.reset({ studentId: '', classOfferingId: value.classOfferingId });
        this.toast.success($localize`:@@academics.enrolled:Student enrolled.`);
      },
      error: (error: unknown) => {
        this.busy.set(false);
        this.errors.set(problemMessages(error));
      },
    });
  }
}
