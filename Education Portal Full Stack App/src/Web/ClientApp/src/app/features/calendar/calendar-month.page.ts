import { ChangeDetectionStrategy, Component, computed, inject, LOCALE_ID, signal } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { CampusFilterComponent, CampusOption } from '../../shared/components/campus-filter/campus-filter.component';
import { PageHeaderComponent } from '../../shared/components/page-header/page-header.component';
import { problemMessages } from '../../core/http/problem-details';
import { CampusesClient, SchoolCalendarDto } from '../../web-api-client';
import { coversDay, dateKey, fromApiDate } from '../academic-calendar/academic-calendar.logic';
import { SchoolCalendarApi } from '../academic-calendar/school-calendar.api';
import { isInMonth, monthGrid, monthTitle, startOfMonth, weekdayNames } from './month-grid';

/** One thing on a day: a holiday, a calendar event or a school event. */
export interface CalendarItem {
  readonly kind: 'holiday' | 'event' | 'school';
  readonly title: string;
}

/** How many items a day cell lists before it says how many more there are. */
const ITEMS_SHOWN = 3;

/**
 * The school calendar as a full-screen month grid (R5.1): holidays, calendar events and school events, with previous and next
 * month, a Today button and a campus filter. Dates are shown as calendar days (no time zone shift). The grid uses Angular
 * Material and the theme's tokens; the week starts on Sunday for every locale.
 */
@Component({
  selector: 'app-calendar-month-page',
  imports: [MatButtonModule, PageHeaderComponent, CampusFilterComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './calendar-month.page.html',
  styleUrl: './calendar-month.page.css',
})
export class CalendarMonthPage {
  private readonly calendarApi = inject(SchoolCalendarApi);
  private readonly campusesClient = inject(CampusesClient);
  private readonly locale = inject(LOCALE_ID);

  protected readonly campuses = signal<readonly CampusOption[]>([]);
  protected readonly campusId = signal<string | null>(null);
  protected readonly anchor = signal(startOfMonth(new Date()));
  protected readonly loadError = signal<string | null>(null);
  protected readonly loading = signal(true);

  /** The items of each day, by `yyyy-MM-dd`. */
  protected readonly itemsByDay = signal(new Map<string, CalendarItem[]>());

  protected readonly weeks = computed(() => monthGrid(this.anchor()));
  protected readonly weekdays = computed(() => weekdayNames(this.locale));
  protected readonly title = computed(() => monthTitle(this.anchor(), this.locale));
  protected readonly shown = ITEMS_SHOWN;

  constructor() {
    this.campusesClient.getCampuses().subscribe({
      next: (campuses) => this.campuses.set(campuses.map((campus) => ({ id: campus.id, name: campus.name }))),
      error: () => this.campuses.set([]),
    });
    this.load();
  }

  /** Reads the whole grid's range (six weeks), so the days from the months before and after are filled too. */
  protected load(): void {
    const weeks = this.weeks();
    const first = weeks[0][0];
    const last = weeks[weeks.length - 1][6];

    this.loading.set(true);
    this.loadError.set(null);
    this.calendarApi.read(first, last, this.campusId()).subscribe({
      next: (calendar) => {
        this.itemsByDay.set(this.indexItems(calendar, weeks.flat()));
        this.loading.set(false);
      },
      error: (error: unknown) => {
        this.loadError.set(problemMessages(error)[0]);
        this.loading.set(false);
      },
    });
  }

  protected items(day: Date): CalendarItem[] {
    return this.itemsByDay().get(dateKey(day)) ?? [];
  }

  protected previousMonth(): void {
    this.move(-1);
  }

  protected nextMonth(): void {
    this.move(1);
  }

  protected today(): void {
    this.anchor.set(startOfMonth(new Date()));
    this.load();
  }

  protected selectCampus(id: string | null): void {
    this.campusId.set(id);
    this.load();
  }

  protected inMonth(day: Date): boolean {
    return isInMonth(day, this.anchor());
  }

  private move(months: number): void {
    const current = this.anchor();
    this.anchor.set(new Date(current.getFullYear(), current.getMonth() + months, 1));
    this.load();
  }

  /** Puts every holiday, event and school event on the days of the grid. Days outside the grid are not needed. */
  private indexItems(calendar: SchoolCalendarDto, days: Date[]): Map<string, CalendarItem[]> {
    const index = new Map<string, CalendarItem[]>();
    const add = (key: string, item: CalendarItem) => {
      const list = index.get(key) ?? [];
      list.push(item);
      index.set(key, list);
    };

    for (const day of days) {
      for (const holiday of calendar.holidays) {
        if (coversDay(fromApiDate(holiday.startDate), fromApiDate(holiday.endDate), day)) {
          add(dateKey(day), { kind: 'holiday', title: holiday.title });
        }
      }
    }
    for (const event of calendar.events) {
      add(dateKey(fromApiDate(event.eventDate)), { kind: 'event', title: event.title });
    }
    for (const event of calendar.schoolEvents) {
      add(dateKey(new Date(event.startsAt)), { kind: 'school', title: event.title });
    }

    return index;
  }
}
