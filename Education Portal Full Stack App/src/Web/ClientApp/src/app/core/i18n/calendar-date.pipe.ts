import { inject, Pipe, PipeTransform } from '@angular/core';
import { LocaleService } from './locale.service';

/** How much of a calendar date to show. */
export type CalendarDateStyle = 'date' | 'full' | 'monthYear';

const OPTIONS: Readonly<Record<CalendarDateStyle, Intl.DateTimeFormatOptions>> = {
  date: { dateStyle: 'medium' },
  full: { dateStyle: 'full' },
  monthYear: { month: 'long', year: 'numeric' },
};

/**
 * Formats a calendar date (a day without a time, such as a holiday or the start of a term) in the page's culture, for
 * example `ur-PK`. Unlike an instant (`localDate`), a calendar date is the same day everywhere, so no time zone is applied:
 * the value is a local date at midnight (see `fromApiDate` in the academic calendar) and is shown as that day.
 */
export function formatCalendarDate(value: Date | null | undefined, culture: string, style: CalendarDateStyle = 'date'): string {
  if (!value || Number.isNaN(value.getTime())) {
    return '';
  }
  // The device's own zone reads a local midnight back as the same day, whichever zone the device is in.
  return new Intl.DateTimeFormat(culture, OPTIONS[style]).format(value);
}

/** `{{ holiday.startDate | calendarDate }}`, `{{ month | calendarDate: 'monthYear' }}`. */
@Pipe({ name: 'calendarDate' })
export class CalendarDatePipe implements PipeTransform {
  private readonly locale = inject(LocaleService);

  transform(value: Date | null | undefined, style: CalendarDateStyle = 'date'): string {
    return formatCalendarDate(value, this.locale.culture, style);
  }
}
