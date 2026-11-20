/**
 * Date helpers for the academic calendar. The API sends calendar dates (`DateOnly`) as `yyyy-MM-dd`; the generated client
 * turns them into a `Date` at midnight UTC, and sends a `Date` back as its local `yyyy-MM-dd`. These helpers keep a
 * calendar date the same day in every time zone: no time zone conversion is ever applied to a date without a time.
 */

/** A date the API sent (midnight UTC) as the same calendar day at local midnight, for the datepicker and the calendar. */
export function fromApiDate(value: Date): Date {
  return new Date(value.getUTCFullYear(), value.getUTCMonth(), value.getUTCDate());
}

/** A local date as `yyyy-MM-dd`, for query strings and for comparing days. */
export function dateKey(value: Date): string {
  const month = String(value.getMonth() + 1).padStart(2, '0');
  const day = String(value.getDate()).padStart(2, '0');
  return `${value.getFullYear()}-${month}-${day}`;
}

/** The first and last day of the month that holds `day`. */
export function monthRange(day: Date): { from: Date; to: Date } {
  return {
    from: new Date(day.getFullYear(), day.getMonth(), 1),
    to: new Date(day.getFullYear(), day.getMonth() + 1, 0),
  };
}

/** True when `day` falls between `start` and `end`, both included (all local dates). */
export function coversDay(start: Date, end: Date, day: Date): boolean {
  const key = dateKey(day);
  return dateKey(start) <= key && key <= dateKey(end);
}

/** A time from an `<input type="time">` (`HH:mm`, or `HH:mm:ss`) as the API's `HH:mm:ss`; blank stays undefined. */
export function toApiTime(value: string | null | undefined): string | undefined {
  const trimmed = (value ?? '').trim();
  if (trimmed === '') {
    return undefined;
  }
  return /^\d{2}:\d{2}$/.test(trimmed) ? `${trimmed}:00` : trimmed;
}

/** A time the API sent (`HH:mm:ss`) as `HH:mm`, for a time input and for display; blank stays ''. */
export function fromApiTime(value: string | null | undefined): string {
  return value ? value.slice(0, 5) : '';
}

/** True when a time range is complete and ends after it starts (`HH:mm` strings compare in order). */
export function isTimeRangeValid(start: string, end: string): boolean {
  return start !== '' && end !== '' && end > start;
}
