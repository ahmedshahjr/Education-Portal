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
import { CourseDto, CoursesClient, CreateCourseCommand } from '../../web-api-client';
import { AcademicsLookups } from './academics-lookups.service';
import { offeringLabel, personName } from './academics.logic';

export const COURSE_NAME_MAX_LENGTH = 100;

/** A course as the table shows it, with its class offering and teacher named. */
interface CourseRow {
  readonly id: string;
  readonly name: string;
  readonly offering: string;
  readonly teacher: string;
}

/** The courses of the school: each belongs to a class offering and is taught by one teacher. */
@Component({
  selector: 'app-courses-panel',
  imports: [ReactiveFormsModule, MatButtonModule, MatCardModule, MatFormFieldModule, MatInputModule, MatSelectModule, DataTableComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './courses-panel.component.html',
  styleUrl: './academics.css',
})
export class CoursesPanelComponent implements OnInit {
  private readonly client = inject(CoursesClient);
  private readonly toast = inject(ToastService);
  protected readonly lookups = inject(AcademicsLookups);

  protected readonly offeringLabel = offeringLabel;
  protected readonly personName = personName;
  protected readonly nameMax = COURSE_NAME_MAX_LENGTH;

  protected readonly form = inject(FormBuilder).nonNullable.group({
    classOfferingId: ['', Validators.required],
    name: ['', [Validators.required, Validators.maxLength(COURSE_NAME_MAX_LENGTH)]],
    teacherId: ['', Validators.required],
  });

  protected readonly busy = signal(false);
  protected readonly adding = signal(false);
  protected readonly errors = signal<readonly string[]>([]);

  protected readonly rows = computed<readonly CourseRow[]>(() => {
    const offerings = new Map(this.lookups.offerings().map((o) => [o.id, offeringLabel(o)]));
    const teachers = new Map(this.lookups.teachers().map((t) => [t.id, personName(t)]));
    return this.lookups.courses().map((course: CourseDto) => ({
      id: course.id,
      name: course.name,
      offering: offerings.get(course.classOfferingId) ?? '',
      teacher: teachers.get(course.teacherId) ?? '',
    }));
  });

  protected readonly columns: DataTableColumn<CourseRow>[] = [
    { key: 'name', header: $localize`:@@academics.colCourse:Course`, valueFn: (c) => c.name },
    { key: 'offering', header: $localize`:@@academics.colOffering:Class`, valueFn: (c) => c.offering },
    { key: 'teacher', header: $localize`:@@academics.colTeacher:Teacher`, valueFn: (c) => c.teacher },
  ];

  ngOnInit(): void {
    this.lookups.loadOfferings();
    this.lookups.loadTeachers();
    this.lookups.loadCourses();
  }

  protected startAdd(): void {
    this.errors.set([]);
    this.form.reset({ classOfferingId: '', name: '', teacherId: '' });
    this.adding.set(true);
  }

  protected save(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const value = this.form.getRawValue();
    this.busy.set(true);
    this.errors.set([]);
    this.client.createCourse(new CreateCourseCommand({ classOfferingId: value.classOfferingId, name: value.name.trim(), teacherId: value.teacherId })).subscribe({
      next: () => {
        this.busy.set(false);
        this.adding.set(false);
        this.toast.success($localize`:@@academics.courseSaved:Course added.`);
        this.lookups.loadCourses();
      },
      error: (error: unknown) => {
        this.busy.set(false);
        this.errors.set(problemMessages(error));
      },
    });
  }
}
