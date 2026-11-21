import { ChangeDetectionStrategy, Component, computed, inject, OnInit, signal, TemplateRef, viewChild } from '@angular/core';
import { AbstractControl, FormBuilder, ReactiveFormsModule, ValidationErrors, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { Observable } from 'rxjs';
import { problemMessages } from '../../core/http/problem-details';
import { AppIconComponent } from '../../shared/components/app-icon/app-icon.component';
import { DataTableCellContext, DataTableColumn, DataTableComponent } from '../../shared/components/data-table/data-table.component';
import { PageHeaderComponent } from '../../shared/components/page-header/page-header.component';
import { ToastService } from '../../shared/services/toast.service';
import {
  CampusDto,
  CampusesClient,
  CheckTimetableConflictsQuery,
  CreateTimetableSlotCommand,
  ITimetableSlotDto,
  TimetableConflictDto,
  TimetableSlotDto,
  TimetableSlotsClient,
  UpdateTimetableSlotCommand,
  Weekday,
} from '../../web-api-client';
import { AcademicsLookups } from '../academics/academics-lookups.service';
import { fromApiTime, isTimeRangeValid, toApiTime } from '../academic-calendar/academic-calendar.logic';
import { compareLessons, conflictLabel, ROOM_MAX_LENGTH, WEEKDAYS, weekdayLabel } from './timetable.logic';

/** A lesson as the table shows it. */
interface LessonRow extends ITimetableSlotDto {
  readonly dayName: string;
  readonly times: string;
  readonly campusName: string;
}

/** The end time must come after the start time. */
function timeRangeValidator(group: AbstractControl): ValidationErrors | null {
  const start = group.get('start')?.value as string;
  const end = group.get('end')?.value as string;
  return start && end && !isTimeRangeValid(start, end) ? { timeRange: true } : null;
}

/**
 * The timetable (Timetable module): the lessons of each course by day and time. A lesson is refused when its teacher, its room
 * or its class is already booked at that time; the form can check that before saving.
 */
@Component({
  selector: 'app-timetable-page',
  imports: [
    ReactiveFormsModule,
    MatButtonModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    AppIconComponent,
    DataTableComponent,
    PageHeaderComponent,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './timetable.page.html',
  styleUrl: '../academics/academics.css',
})
export class TimetablePage implements OnInit {
  private readonly client = inject(TimetableSlotsClient);
  private readonly campusesClient = inject(CampusesClient);
  private readonly toast = inject(ToastService);
  protected readonly lookups = inject(AcademicsLookups);

  protected readonly days = WEEKDAYS;
  protected readonly dayLabel = weekdayLabel;
  protected readonly conflictLabel = conflictLabel;
  protected readonly roomMax = ROOM_MAX_LENGTH;

  protected readonly slots = signal<readonly TimetableSlotDto[] | null>(null);
  protected readonly campuses = signal<readonly CampusDto[]>([]);
  protected readonly loadError = signal<string | null>(null);
  /** The lesson whose form is open: 'new', its id, or null. */
  protected readonly editing = signal<string | null>(null);
  protected readonly busy = signal(false);
  protected readonly errors = signal<readonly string[]>([]);
  protected readonly conflicts = signal<readonly TimetableConflictDto[] | null>(null);

  protected readonly rows = computed<readonly LessonRow[]>(() => {
    const campuses = new Map(this.campuses().map((campus) => [campus.id, campus.name]));
    return [...(this.slots() ?? [])]
      .sort((a, b) => compareLessons({ day: a.day, start: a.start }, { day: b.day, start: b.start }))
      .map((slot) => ({
        ...slot,
        dayName: weekdayLabel(slot.day),
        times: `${fromApiTime(slot.start)} - ${fromApiTime(slot.end)}`,
        campusName: slot.campusId ? (campuses.get(slot.campusId) ?? '') : $localize`:@@timetable.wholeSchool:Whole school`,
      }));
  });

  private readonly actionsCell = viewChild.required<TemplateRef<DataTableCellContext<LessonRow>>>('actionsCell');

  protected readonly columns = computed<DataTableColumn<LessonRow>[]>(() => [
    { key: 'day', header: $localize`:@@timetable.colDay:Day`, valueFn: (l) => l.dayName },
    { key: 'times', header: $localize`:@@timetable.colTime:Time`, valueFn: (l) => l.times },
    { key: 'course', header: $localize`:@@timetable.colCourse:Course`, valueFn: (l) => l.courseName },
    { key: 'campus', header: $localize`:@@timetable.colCampus:Campus`, valueFn: (l) => l.campusName },
    { key: 'room', header: $localize`:@@timetable.colRoom:Room`, valueFn: (l) => l.room ?? '' },
    { key: 'actions', header: $localize`:@@common.actions:Actions`, sortable: false, align: 'right', cell: this.actionsCell() },
  ]);

  protected readonly form = inject(FormBuilder).group(
    {
      courseId: ['', Validators.required],
      day: [Weekday.Monday as Weekday, Validators.required],
      start: ['', Validators.required],
      end: ['', Validators.required],
      campusId: [null as string | null],
      room: ['', Validators.maxLength(ROOM_MAX_LENGTH)],
    },
    { validators: [timeRangeValidator] }
  );

  ngOnInit(): void {
    this.lookups.loadCourses();
    this.campusesClient.getCampuses().subscribe({ next: (campuses) => this.campuses.set(campuses), error: () => this.campuses.set([]) });
    this.load();
  }

  protected load(): void {
    this.loadError.set(null);
    this.client.getTimetableSlots(undefined).subscribe({
      next: (slots) => this.slots.set(slots),
      error: (error: unknown) => {
        this.slots.set([]);
        this.loadError.set(problemMessages(error)[0]);
      },
    });
  }

  protected startAdd(): void {
    this.errors.set([]);
    this.conflicts.set(null);
    this.form.reset({ courseId: '', day: Weekday.Monday, start: '', end: '', campusId: null, room: '' });
    this.form.controls.courseId.enable();
    this.editing.set('new');
  }

  protected startEdit(lesson: ITimetableSlotDto): void {
    this.errors.set([]);
    this.conflicts.set(null);
    this.form.reset({
      courseId: lesson.courseId,
      day: lesson.day,
      start: fromApiTime(lesson.start),
      end: fromApiTime(lesson.end),
      campusId: lesson.campusId ?? null,
      room: lesson.room ?? '',
    });
    // A lesson stays with its course; to change that, delete it from the timetable and add it again.
    this.form.controls.courseId.disable();
    this.editing.set(lesson.id);
  }

  protected cancel(): void {
    this.errors.set([]);
    this.conflicts.set(null);
    this.editing.set(null);
  }

  /** Asks the server whether the lesson as typed clashes with another one, without saving it. */
  protected check(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const value = this.form.getRawValue();
    const editing = this.editing();
    this.busy.set(true);
    this.errors.set([]);
    this.client
      .checkTimetableConflicts(
        new CheckTimetableConflictsQuery({
          courseId: value.courseId ?? '',
          day: value.day ?? Weekday.Monday,
          start: toApiTime(value.start),
          end: toApiTime(value.end),
          campusId: value.campusId ?? undefined,
          room: value.room?.trim() || undefined,
          slotId: editing !== null && editing !== 'new' ? editing : undefined,
        })
      )
      .subscribe({
        next: (conflicts) => {
          this.busy.set(false);
          this.conflicts.set(conflicts);
        },
        error: (error: unknown) => {
          this.busy.set(false);
          this.errors.set(problemMessages(error));
        },
      });
  }

  protected save(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const value = this.form.getRawValue();
    const editing = this.editing();
    const when = {
      day: value.day ?? Weekday.Monday,
      start: toApiTime(value.start),
      end: toApiTime(value.end),
      campusId: value.campusId ?? undefined,
      room: value.room?.trim() || undefined,
    };
    const request: Observable<unknown> =
      editing === 'new'
        ? this.client.createTimetableSlot(new CreateTimetableSlotCommand({ courseId: value.courseId ?? '', ...when }))
        : this.client.updateTimetableSlot(editing ?? '', new UpdateTimetableSlotCommand(when));

    this.busy.set(true);
    this.errors.set([]);
    request.subscribe({
      next: () => {
        this.busy.set(false);
        this.toast.success($localize`:@@timetable.saved:Lesson saved.`);
        this.editing.set(null);
        this.conflicts.set(null);
        this.load();
      },
      error: (error: unknown) => {
        this.busy.set(false);
        this.errors.set(problemMessages(error));
      },
    });
  }
}
