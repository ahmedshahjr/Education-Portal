import { NgTemplateOutlet } from '@angular/common';
import {
  afterNextRender,
  ChangeDetectionStrategy,
  Component,
  computed,
  DestroyRef,
  effect,
  inject,
  input,
  signal,
  viewChild,
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { AbstractControl, FormBuilder, ReactiveFormsModule, ValidationErrors, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatCalendar, MatCalendarCellClassFunction, MatDatepickerModule } from '@angular/material/datepicker';
import { MatDialog } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { Observable } from 'rxjs';
import { problemMessages } from '../../core/http/problem-details';
import { CalendarDatePipe } from '../../core/i18n/calendar-date.pipe';
import { AppIconComponent } from '../../shared/components/app-icon/app-icon.component';
import { CampusFilterComponent, CampusOption } from '../../shared/components/campus-filter/campus-filter.component';
import { ConfirmDialogComponent, ConfirmDialogData } from '../../shared/components/confirm-dialog/confirm-dialog.component';
import { EmptyStateComponent } from '../../shared/components/empty-state/empty-state.component';
import { StatusBadgeComponent } from '../../shared/components/status-badge/status-badge.component';
import { ToastService } from '../../shared/services/toast.service';
import {
  CalendarEventAudience,
  CalendarEventDto,
  CalendarEventType,
  CreateCalendarEventCommand,
  CreateHolidayCommand,
  HolidayAudience,
  HolidayDto,
  HolidayType,
  SchoolCalendarClient,
  UpdateCalendarEventCommand,
  UpdateHolidayCommand,
} from '../../web-api-client';
import {
  eventAudienceOptions,
  eventTypeOptions,
  holidayAudienceOptions,
  holidayTypeOptions,
  labelOf,
} from './academic-calendar.labels';
import { coversDay, dateKey, fromApiDate, fromApiTime, isTimeRangeValid, monthRange, toApiTime } from './academic-calendar.logic';
import { SchoolCalendarApi } from './school-calendar.api';

/** A holiday or an event as the calendar shows it, with local calendar dates. */
export interface CalendarEntry {
  readonly kind: 'holiday' | 'event';
  readonly id: string;
  readonly title: string;
  readonly campusId: string | null;
  readonly startDate: Date;
  readonly endDate: Date;
  readonly typeLabel: string;
  readonly audienceLabel: string;
  readonly timeLabel: string;
  readonly isTentative: boolean;
  readonly holiday?: HolidayDto;
  readonly event?: CalendarEventDto;
}

function holidayEntry(holiday: HolidayDto): CalendarEntry {
  return {
    kind: 'holiday',
    id: holiday.id,
    title: holiday.title,
    campusId: holiday.campusId ?? null,
    startDate: fromApiDate(holiday.startDate),
    endDate: fromApiDate(holiday.endDate),
    typeLabel: labelOf(holidayTypeOptions(), holiday.type),
    audienceLabel: labelOf(holidayAudienceOptions(), holiday.appliesTo),
    timeLabel: '',
    isTentative: holiday.isTentative,
    holiday,
  };
}

function eventEntry(event: CalendarEventDto): CalendarEntry {
  const day = fromApiDate(event.eventDate);
  const start = fromApiTime(event.startTime);
  const end = fromApiTime(event.endTime);
  return {
    kind: 'event',
    id: event.id,
    title: event.title,
    campusId: event.campusId ?? null,
    startDate: day,
    endDate: day,
    typeLabel: labelOf(eventTypeOptions(), event.type),
    audienceLabel: labelOf(eventAudienceOptions(), event.audience),
    timeLabel: start === '' ? '' : end === '' ? start : `${start} – ${end}`,
    isTentative: false,
    event,
  };
}

/** An event's end time needs a start time and must come after it. */
function eventTimesValidator(group: AbstractControl): ValidationErrors | null {
  const start = group.get('startTime')?.value as string;
  const end = group.get('endTime')?.value as string;
  if (end && !start) {
    return { endNeedsStart: true };
  }
  return start && end && !isTimeRangeValid(start, end) ? { timeRange: true } : null;
}

/**
 * The school calendar: Angular Material's calendar (the theme has no calendar of its own, so it is drawn with the theme's
 * tokens) marks the days with a holiday or an event. Choosing a day lists what happens on it; the month's holidays and events
 * are listed beside it. A holiday or an event is for the whole school or for one campus.
 */
@Component({
  selector: 'app-school-calendar-panel',
  imports: [
    CalendarDatePipe,
    NgTemplateOutlet,
    ReactiveFormsModule,
    MatButtonModule,
    MatCardModule,
    MatCheckboxModule,
    MatDatepickerModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    AppIconComponent,
    CampusFilterComponent,
    EmptyStateComponent,
    StatusBadgeComponent,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './school-calendar-panel.component.html',
  styleUrl: './academic-calendar.page.css',
})
export class SchoolCalendarPanelComponent {
  private readonly api = inject(SchoolCalendarApi);
  private readonly client = inject(SchoolCalendarClient);
  private readonly formBuilder = inject(FormBuilder);
  private readonly toast = inject(ToastService);
  private readonly dialog = inject(MatDialog);
  private readonly destroyRef = inject(DestroyRef);

  /** The school's campuses, loaded once by the page. */
  readonly campuses = input.required<readonly CampusOption[]>();

  private readonly calendar = viewChild(MatCalendar<Date>);

  readonly selectedDay = signal<Date>(new Date(new Date().getFullYear(), new Date().getMonth(), new Date().getDate()));
  /** The first day of the month the calendar shows. */
  readonly month = signal<Date>(monthRange(new Date()).from);
  readonly campusId = signal<string | null>(null);

  readonly entries = signal<readonly CalendarEntry[] | null>(null);
  protected readonly loadError = signal<string | null>(null);
  protected readonly busy = signal(false);
  protected readonly errors = signal<readonly string[]>([]);

  /** The form that is open: a new or existing holiday or event, or null. */
  readonly editing = signal<{ kind: 'holiday' | 'event'; id: string | null } | null>(null);

  protected readonly holidayTypes = holidayTypeOptions();
  protected readonly holidayAudiences = holidayAudienceOptions();
  protected readonly eventTypes = eventTypeOptions();
  protected readonly eventAudiences = eventAudienceOptions();

  /** What happens on the chosen day. */
  readonly dayEntries = computed(() => (this.entries() ?? []).filter((entry) => coversDay(entry.startDate, entry.endDate, this.selectedDay())));

  /** The calendar marks a day with a holiday, an event, or both. */
  protected readonly dateClass: MatCalendarCellClassFunction<Date> = (date, view) => {
    if (view !== 'month') {
      return '';
    }
    const onDay = (this.entries() ?? []).filter((entry) => coversDay(entry.startDate, entry.endDate, date));
    return {
      'mm-calendar-day--holiday': onDay.some((entry) => entry.kind === 'holiday'),
      'mm-calendar-day--event': onDay.some((entry) => entry.kind === 'event'),
    };
  };

  readonly holidayForm = this.formBuilder.group({
    title: this.formBuilder.nonNullable.control('', [Validators.required, Validators.maxLength(200)]),
    campusId: this.formBuilder.nonNullable.control(''),
    startDate: this.formBuilder.control<Date | null>(null, Validators.required),
    endDate: this.formBuilder.control<Date | null>(null, Validators.required),
    type: this.formBuilder.nonNullable.control(HolidayType.Public),
    appliesTo: this.formBuilder.nonNullable.control(HolidayAudience.Both),
    isTentative: this.formBuilder.nonNullable.control(false),
  });

  readonly eventForm = this.formBuilder.group(
    {
      title: this.formBuilder.nonNullable.control('', [Validators.required, Validators.maxLength(200)]),
      description: this.formBuilder.nonNullable.control('', [Validators.maxLength(2000)]),
      campusId: this.formBuilder.nonNullable.control(''),
      eventDate: this.formBuilder.control<Date | null>(null, Validators.required),
      startTime: this.formBuilder.nonNullable.control(''),
      endTime: this.formBuilder.nonNullable.control(''),
      type: this.formBuilder.nonNullable.control(CalendarEventType.Other),
      audience: this.formBuilder.nonNullable.control(CalendarEventAudience.All),
    },
    { validators: [eventTimesValidator] }
  );

  constructor() {
    // Reload whenever the month or the campus changes.
    effect(() => this.load(this.month(), this.campusId()));

    // The calendar reports month changes (the arrows, the period picker) through its active date.
    afterNextRender(() => {
      const calendar = this.calendar();
      calendar?.stateChanges.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(() => {
        const shown = monthRange(calendar.activeDate).from;
        if (dateKey(shown) !== dateKey(this.month())) {
          this.month.set(shown);
        }
      });
    });
  }

  reload(): void {
    this.load(this.month(), this.campusId());
  }

  private load(month: Date, campusId: string | null): void {
    this.loadError.set(null);
    const { from, to } = monthRange(month);
    this.api.read(from, to, campusId).subscribe({
      next: (calendar) => {
        const entries = [...calendar.holidays.map(holidayEntry), ...calendar.events.map(eventEntry)].sort(
          (a, b) => dateKey(a.startDate).localeCompare(dateKey(b.startDate)) || a.title.localeCompare(b.title)
        );
        this.entries.set(entries);
        // The cells read their marks when drawn; draw them again with the new entries.
        this.calendar()?.updateTodaysDate();
      },
      error: (error: unknown) => this.loadError.set(problemMessages(error)[0]),
    });
  }

  protected campusName(campusId: string | null): string {
    if (campusId === null) {
      return $localize`:@@academicCalendar.wholeSchool:Whole school`;
    }
    return this.campuses().find((campus) => campus.id === campusId)?.name ?? '';
  }

  selectDay(day: Date | null): void {
    if (day) {
      this.selectedDay.set(day);
    }
  }

  startAddHoliday(): void {
    this.errors.set([]);
    const day = this.selectedDay();
    this.holidayForm.reset({
      title: '',
      campusId: this.campusId() ?? '',
      startDate: day,
      endDate: day,
      type: HolidayType.Public,
      appliesTo: HolidayAudience.Both,
      isTentative: false,
    });
    this.editing.set({ kind: 'holiday', id: null });
  }

  startAddEvent(): void {
    this.errors.set([]);
    this.eventForm.reset({
      title: '',
      description: '',
      campusId: this.campusId() ?? '',
      eventDate: this.selectedDay(),
      startTime: '',
      endTime: '',
      type: CalendarEventType.Other,
      audience: CalendarEventAudience.All,
    });
    this.editing.set({ kind: 'event', id: null });
  }

  startEdit(entry: CalendarEntry): void {
    this.errors.set([]);
    if (entry.holiday) {
      const holiday = entry.holiday;
      this.holidayForm.reset({
        title: holiday.title,
        campusId: holiday.campusId ?? '',
        startDate: entry.startDate,
        endDate: entry.endDate,
        type: holiday.type,
        appliesTo: holiday.appliesTo,
        isTentative: holiday.isTentative,
      });
    } else if (entry.event) {
      const event = entry.event;
      this.eventForm.reset({
        title: event.title,
        description: event.description ?? '',
        campusId: event.campusId ?? '',
        eventDate: entry.startDate,
        startTime: fromApiTime(event.startTime),
        endTime: fromApiTime(event.endTime),
        type: event.type,
        audience: event.audience,
      });
    }
    this.editing.set({ kind: entry.kind, id: entry.id });
  }

  cancel(): void {
    this.errors.set([]);
    this.editing.set(null);
  }

  saveHoliday(): void {
    const value = this.holidayForm.getRawValue();
    const editing = this.editing();
    if (this.holidayForm.invalid || value.startDate === null || value.endDate === null || editing === null) {
      this.holidayForm.markAllAsTouched();
      return;
    }
    const body = {
      title: value.title.trim(),
      campusId: value.campusId || undefined,
      startDate: value.startDate,
      endDate: value.endDate,
      type: value.type,
      appliesTo: value.appliesTo,
      isTentative: value.isTentative,
    };
    const request: Observable<unknown> =
      editing.id === null
        ? this.client.createHoliday(undefined, new CreateHolidayCommand(body))
        : this.client.updateHoliday(undefined, editing.id, new UpdateHolidayCommand(body));
    this.run(request, $localize`:@@academicCalendar.holidaySaved:Holiday saved.`, () => this.editing.set(null));
  }

  saveEvent(): void {
    const value = this.eventForm.getRawValue();
    const editing = this.editing();
    if (this.eventForm.invalid || value.eventDate === null || editing === null) {
      this.eventForm.markAllAsTouched();
      return;
    }
    const body = {
      title: value.title.trim(),
      description: value.description.trim() || undefined,
      campusId: value.campusId || undefined,
      eventDate: value.eventDate,
      startTime: toApiTime(value.startTime),
      endTime: toApiTime(value.endTime),
      type: value.type,
      audience: value.audience,
    };
    const request: Observable<unknown> =
      editing.id === null
        ? this.client.createCalendarEvent(undefined, new CreateCalendarEventCommand(body))
        : this.client.updateCalendarEvent(undefined, editing.id, new UpdateCalendarEventCommand(body));
    this.run(request, $localize`:@@academicCalendar.eventSaved:Event saved.`, () => this.editing.set(null));
  }

  confirmDelete(entry: CalendarEntry): void {
    const data: ConfirmDialogData =
      entry.kind === 'holiday'
        ? {
            title: $localize`:@@academicCalendar.deleteHolidayTitle:Delete this holiday?`,
            message: $localize`:@@academicCalendar.deleteHolidayMessage:The holiday is removed from the calendar.`,
            confirmLabel: $localize`:@@academicCalendar.delete:Delete`,
          }
        : {
            title: $localize`:@@academicCalendar.deleteEventTitle:Delete this event?`,
            message: $localize`:@@academicCalendar.deleteEventMessage:The event is removed from the calendar.`,
            confirmLabel: $localize`:@@academicCalendar.delete:Delete`,
          };
    this.dialog
      .open(ConfirmDialogComponent, { data, autoFocus: 'dialog' })
      .afterClosed()
      .subscribe((confirmed: boolean | undefined) => {
        if (confirmed !== true) {
          return;
        }
        const request =
          entry.kind === 'holiday'
            ? this.client.deleteHoliday(undefined, entry.id)
            : this.client.deleteCalendarEvent(undefined, entry.id);
        this.run(request, $localize`:@@academicCalendar.entryDeleted:Removed from the calendar.`);
      });
  }

  private run(request: Observable<unknown>, done: string, after?: () => void): void {
    this.busy.set(true);
    this.errors.set([]);
    request.subscribe({
      next: () => {
        this.busy.set(false);
        this.toast.success(done);
        after?.();
        this.reload();
      },
      error: (error: unknown) => {
        this.busy.set(false);
        this.errors.set(problemMessages(error));
      },
    });
  }
}
