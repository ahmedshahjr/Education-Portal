import { inject, Pipe, PipeTransform } from '@angular/core';
import { LocaleService } from './locale.service';

/** How much of an instant to show. */
export type LocalDateStyle = 'date' | 'dateTime' | 'time';

const OPTIONS: Readonly<Record<LocalDateStyle, Intl.DateTimeFormatOptions>> = {
  date: { dateStyle: 'medium' },
  dateTime: { dateStyle: 'medium', timeStyle: 'short' },
  time: { timeStyle: 'short' },
};

/**
 * Formats a UTC instant from the API in the user's time zone (their device's, else the school's) and the page's
 * culture, for example `ur-PK` (ADR-005). Use it for every date or time on screen instead of Angular's `date` pipe,
 * which only takes fixed offsets such as `+0500`, not IANA time zones, so it would be wrong across daylight saving.
 */
export function formatLocalDate(
  value: Date | string | null | undefined,
  culture: string,
  timeZone: string,
  style: LocalDateStyle = 'date'
): string {
  if (value === null || value === undefined || value === '') {
    return '';
  }

  const instant = value instanceof Date ? value : new Date(value);
  if (Number.isNaN(instant.getTime())) {
    return '';
  }

  try {
    return new Intl.DateTimeFormat(culture, { ...OPTIONS[style], timeZone }).format(instant);
  } catch {
    // An unknown time zone in this browser: UTC, rather than the device's zone, so it never silently differs from the server.
    return new Intl.DateTimeFormat(culture, { ...OPTIONS[style], timeZone: 'UTC' }).format(instant);
  }
}

/** `{{ enrolment.created | localDate }}`, `{{ grade.recordedAt | localDate: 'dateTime' }}`. */
@Pipe({ name: 'localDate' })
export class LocalDatePipe implements PipeTransform {
  private readonly locale = inject(LocaleService);

  transform(value: Date | string | null | undefined, style: LocalDateStyle = 'date'): string {
    return formatLocalDate(value, this.locale.culture, this.locale.userTimeZone, style);
  }
}
