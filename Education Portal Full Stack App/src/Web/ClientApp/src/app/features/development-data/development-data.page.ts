import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { ReactiveFormsModule, FormControl, FormGroup, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { ToastService } from '../../shared/services/toast.service';
import { problemMessages } from '../../core/http/problem-details';
import { AppIconComponent } from '../../shared/components/app-icon/app-icon.component';
import { PageHeaderComponent } from '../../shared/components/page-header/page-header.component';
import { DevelopmentDataClient, DevelopmentDataSummary, GenerateDevelopmentDataCommand } from '../../web-api-client';

/** The limits the server checks (GenerateDevelopmentDataCommandValidator). The form shows them; the server still decides. */
const MAXIMUM_SCHOOLS = 500;
const MAXIMUM_CAMPUSES_PER_SCHOOL = 50;
const MAXIMUM_PEOPLE_PER_CAMPUS = 5000;
const MAXIMUM_PEOPLE = 1_000_000;

/**
 * Master portal, Development only: creates fake schools, campuses and people in one call through
 * `POST /api/DevelopmentData/generate`. Outside Development the server answers 404 and the page shows that message.
 * Master tenant administrators only; the server enforces it.
 */
@Component({
  selector: 'app-development-data-page',
  imports: [ReactiveFormsModule, MatCardModule, MatButtonModule, MatFormFieldModule, MatInputModule, PageHeaderComponent, AppIconComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './development-data.page.html',
  styleUrl: './development-data.page.css',
})
export class DevelopmentDataPage {
  private readonly client = inject(DevelopmentDataClient);
  private readonly toast = inject(ToastService);

  protected readonly maximumSchools = MAXIMUM_SCHOOLS;
  protected readonly maximumCampusesPerSchool = MAXIMUM_CAMPUSES_PER_SCHOOL;
  protected readonly maximumPeoplePerCampus = MAXIMUM_PEOPLE_PER_CAMPUS;
  protected readonly maximumPeople = MAXIMUM_PEOPLE;

  readonly form = new FormGroup({
    schools: new FormControl(3, { nonNullable: true, validators: [Validators.required, Validators.min(1), Validators.max(MAXIMUM_SCHOOLS)] }),
    campusesPerSchool: new FormControl(2, {
      nonNullable: true,
      validators: [Validators.required, Validators.min(1), Validators.max(MAXIMUM_CAMPUSES_PER_SCHOOL)],
    }),
    peoplePerCampus: new FormControl(20, {
      nonNullable: true,
      validators: [Validators.required, Validators.min(0), Validators.max(MAXIMUM_PEOPLE_PER_CAMPUS)],
    }),
    countryCode: new FormControl('PK', { nonNullable: true, validators: [Validators.required, Validators.pattern(/^[A-Za-z]{2}$/)] }),
    seed: new FormControl<number | null>(null, { validators: [Validators.min(0)] }),
  });

  readonly saving = signal(false);
  protected readonly errors = signal<readonly string[]>([]);
  readonly summary = signal<DevelopmentDataSummary | null>(null);

  /** People the request would create, from the form values (the product the server limits). */
  readonly totalPeople = signal(this.calculateTotal());

  /** Whether the total is within the server's limit, so the form can say so before sending. */
  readonly withinLimit = computed(() => this.totalPeople() <= MAXIMUM_PEOPLE);

  readonly cannotSubmit = computed(() => this.formValid() === false || !this.withinLimit());

  private readonly formValid = signal(this.form.valid);

  constructor() {
    this.form.valueChanges.subscribe(() => {
      this.totalPeople.set(this.calculateTotal());
      this.formValid.set(this.form.valid);
    });
  }

  generate(): void {
    if (this.cannotSubmit() || this.saving()) {
      this.form.markAllAsTouched();
      return;
    }

    const value = this.form.getRawValue();
    const command = new GenerateDevelopmentDataCommand({
      schools: value.schools,
      campusesPerSchool: value.campusesPerSchool,
      peoplePerCampus: value.peoplePerCampus,
      countryCode: value.countryCode.toUpperCase(),
      seed: value.seed ?? undefined,
    });

    this.saving.set(true);
    this.errors.set([]);
    this.client.generateDevelopmentData(command).subscribe({
      next: (result) => {
        this.saving.set(false);
        this.summary.set(result);
        this.toast.success($localize`:@@devData.done:Development data created.`);
      },
      error: (error: unknown) => {
        this.saving.set(false);
        this.errors.set(problemMessages(error));
      },
    });
  }

  private calculateTotal(): number {
    const value = this.form?.getRawValue();
    if (!value) {
      return 0;
    }
    return value.schools * value.campusesPerSchool * value.peoplePerCampus;
  }
}
