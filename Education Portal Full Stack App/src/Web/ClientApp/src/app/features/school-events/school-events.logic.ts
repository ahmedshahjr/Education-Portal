import { EventAudience } from '../../web-api-client';

export const EVENT_TITLE_MAX_LENGTH = 200;
export const EVENT_DESCRIPTION_MAX_LENGTH = 2000;

export const EVENT_AUDIENCES: readonly EventAudience[] = Object.values(EventAudience);

/** Who an event is for, in the page's language. */
export function audienceLabel(audience: EventAudience): string {
  switch (audience) {
    case EventAudience.All:
      return $localize`:@@events.audienceAll:Everyone`;
    case EventAudience.Staff:
      return $localize`:@@events.audienceStaff:Staff`;
    case EventAudience.Parents:
      return $localize`:@@events.audienceParents:Parents`;
    case EventAudience.Students:
      return $localize`:@@events.audienceStudents:Students`;
  }
}

/** The first day of the month of `date` and the first day of the next month, the range the page shows to begin with. */
export function monthRange(date: Date): { from: Date; to: Date } {
  return { from: new Date(date.getFullYear(), date.getMonth(), 1), to: new Date(date.getFullYear(), date.getMonth() + 1, 1) };
}

/** A date as the value of a date input: `yyyy-MM-dd`, in the browser's time zone. */
export function toDateInput(date: Date): string {
  const month = String(date.getMonth() + 1).padStart(2, '0');
  const day = String(date.getDate()).padStart(2, '0');
  return `${date.getFullYear()}-${month}-${day}`;
}

/** The start of the day in a date input's value (`yyyy-MM-dd`), or `null` when it is not a date. */
export function fromDateInput(value: string): Date | null {
  const match = /^(\d{4})-(\d{2})-(\d{2})$/.exec(value);
  if (!match) {
    return null;
  }

  const date = new Date(Number(match[1]), Number(match[2]) - 1, Number(match[3]));
  return Number.isNaN(date.getTime()) ? null : date;
}

/** An event must end after it starts. Both are the values of datetime-local inputs (shared rule, see date-validators). */
export { endsAfterStart } from '../../shared/validation/date-validators';
