import { ChangeDetectionStrategy, Component, computed, inject, OnInit, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { problemMessages } from '../../core/http/problem-details';
import { DataTableColumn, DataTableComponent } from '../../shared/components/data-table/data-table.component';
import { ToastService } from '../../shared/services/toast.service';
import { AssessmentDto, AssessmentsClient, CreateAssessmentCommand, RecordScoreCommand, ScoresClient } from '../../web-api-client';
import { AcademicsLookups } from '../academics/academics-lookups.service';
import { personName } from '../academics/academics.logic';
import { scorePercent } from './exams.logic';

export const ASSESSMENT_NAME_MAX_LENGTH = 100;

/** An assessment as the table shows it. */
interface AssessmentRow {
  readonly id: string;
  readonly name: string;
  readonly weight: string;
  readonly maximumScore: string;
}

/**
 * The assessments of a course (an exam, a quiz...) with their weight and maximum score, and the scores of students in them. The
 * weights of a course cannot add up to more than 100; the server checks that, and that the user may grade the course.
 */
@Component({
  selector: 'app-assessments-panel',
  imports: [ReactiveFormsModule, MatButtonModule, MatCardModule, MatFormFieldModule, MatInputModule, MatSelectModule, DataTableComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './assessments-panel.component.html',
  styleUrl: '../academics/academics.css',
})
export class AssessmentsPanelComponent implements OnInit {
  private readonly assessmentsClient = inject(AssessmentsClient);
  private readonly scoresClient = inject(ScoresClient);
  private readonly toast = inject(ToastService);
  private readonly formBuilder = inject(FormBuilder);
  protected readonly lookups = inject(AcademicsLookups);

  protected readonly personName = personName;
  protected readonly nameMax = ASSESSMENT_NAME_MAX_LENGTH;

  protected readonly courseControl = this.formBuilder.nonNullable.control('', Validators.required);
  protected readonly courseId = signal('');
  protected readonly assessments = signal<readonly AssessmentDto[] | null>(null);
  protected readonly adding = signal(false);
  protected readonly busy = signal(false);
  protected readonly errors = signal<readonly string[]>([]);

  protected readonly assessmentForm = this.formBuilder.nonNullable.group({
    name: ['', [Validators.required, Validators.maxLength(ASSESSMENT_NAME_MAX_LENGTH)]],
    weight: [0, [Validators.required, Validators.min(0), Validators.max(100)]],
    maximumScore: [100, [Validators.required, Validators.min(0.01)]],
  });

  protected readonly scoreForm = this.formBuilder.nonNullable.group({
    assessmentId: ['', Validators.required],
    studentId: ['', Validators.required],
    value: [0, [Validators.required, Validators.min(0)]],
  });

  private readonly scoreAssessment = signal('');
  private readonly scoreValue = signal(0);

  protected readonly rows = computed<readonly AssessmentRow[]>(() =>
    (this.assessments() ?? []).map((a) => ({ id: a.id, name: a.name, weight: `${a.weight}%`, maximumScore: String(a.maximumScore) }))
  );

  protected readonly columns: DataTableColumn<AssessmentRow>[] = [
    { key: 'name', header: $localize`:@@exams.colAssessment:Assessment`, valueFn: (a) => a.name },
    { key: 'weight', header: $localize`:@@exams.colWeight:Weight`, valueFn: (a) => a.weight },
    { key: 'maximum', header: $localize`:@@exams.colMaximum:Maximum score`, valueFn: (a) => a.maximumScore },
  ];

  /** What the score typed so far is worth in percent, shown beside the field. */
  protected readonly percent = computed(() => {
    const assessment = (this.assessments() ?? []).find((a) => a.id === this.scoreAssessment());
    return assessment ? scorePercent(this.scoreValue(), assessment.maximumScore) : null;
  });

  ngOnInit(): void {
    this.lookups.loadCourses();
    this.lookups.loadStudents();
    this.scoreForm.controls.assessmentId.valueChanges.subscribe((id) => this.scoreAssessment.set(id));
    this.scoreForm.controls.value.valueChanges.subscribe((value) => this.scoreValue.set(Number(value) || 0));
  }

  protected chooseCourse(courseId: string): void {
    this.courseId.set(courseId);
    this.adding.set(false);
    this.scoreForm.reset({ assessmentId: '', studentId: '', value: 0 });
    this.loadAssessments();
  }

  private loadAssessments(): void {
    this.assessments.set(null);
    this.assessmentsClient.getAssessments(this.courseId()).subscribe({
      next: (assessments) => this.assessments.set(assessments),
      error: (error: unknown) => {
        this.assessments.set([]);
        this.errors.set(problemMessages(error));
      },
    });
  }

  protected startAdd(): void {
    this.errors.set([]);
    this.assessmentForm.reset({ name: '', weight: 0, maximumScore: 100 });
    this.adding.set(true);
  }

  protected saveAssessment(): void {
    if (this.assessmentForm.invalid) {
      this.assessmentForm.markAllAsTouched();
      return;
    }

    const value = this.assessmentForm.getRawValue();
    this.busy.set(true);
    this.errors.set([]);
    this.assessmentsClient
      .createAssessment(new CreateAssessmentCommand({ courseId: this.courseId(), name: value.name.trim(), weight: value.weight, maximumScore: value.maximumScore }))
      .subscribe({
        next: () => {
          this.busy.set(false);
          this.adding.set(false);
          this.toast.success($localize`:@@exams.assessmentSaved:Assessment added.`);
          this.loadAssessments();
        },
        error: (error: unknown) => {
          this.busy.set(false);
          this.errors.set(problemMessages(error));
        },
      });
  }

  protected saveScore(): void {
    if (this.scoreForm.invalid) {
      this.scoreForm.markAllAsTouched();
      return;
    }

    const value = this.scoreForm.getRawValue();
    this.busy.set(true);
    this.errors.set([]);
    this.scoresClient.recordScore(value.assessmentId, value.studentId, new RecordScoreCommand({ value: Number(value.value) })).subscribe({
      next: () => {
        this.busy.set(false);
        this.toast.success($localize`:@@exams.scoreSaved:Score saved.`);
        this.scoreForm.patchValue({ value: 0 });
      },
      error: (error: unknown) => {
        this.busy.set(false);
        this.errors.set(problemMessages(error));
      },
    });
  }
}
