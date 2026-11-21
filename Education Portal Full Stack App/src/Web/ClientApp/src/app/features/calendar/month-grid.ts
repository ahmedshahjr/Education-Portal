/**
 * Month grid helpers for the full-screen calendar (R5.1). A month is six weeks of seven days, Sunday first, so the grid always
 * has the same size. Dates are local calendar days; no time zone conversion is applied (see academic-calendar.logic.ts).
 */

/** The first day of the month that holds `day`, at local midnight. */
export function startOfMonth(day: Date): Date {
  return new Date(day.getFullYear(), day.getMonth(), 1);
}

/** The six weeks shown for the month of `anchor`: 42 local days, starting on the Sunday on or before the 1st. */
export function monthGrid(anchor: Date): Date[][] {
  const first = startOfMonth(anchor);
  const start = new Date(first.getFullYear(), first.getMonth(), 1 - first.getDay());

  const weeks: Date[][] = [];
  for (let week = 0; week < 6; week++) {
    const days: Date[] = [];
    for (let day = 0; day < 7; day++) {
      days.push(new Date(start.getFullYear(), start.getMonth(), start.getDate() + week * 7 + day));
    }
    weeks.push(days);
  }
  return weeks;
}

/** True when the day is in the month of `anchor` (the grid also shows days of the months before and after). */
export function isInMonth(day: Date, anchor: Date): boolean {
  return day.getMonth() === anchor.getMonth() && day.getFullYear() === anchor.getFullYear();
}

/** The Sunday-first weekday names of the grid, formatted for a locale (for example `Sun` in English). */
export function weekdayNames(locale: string): string[] {
  const formatter = new Intl.DateTimeFormat(locale, { weekday: 'short' });
  // 2026-11-01 is a Sunday, so the first seven days are the grid's columns.
  return Array.from({ length: 7 }, (_, index) => formatter.format(new Date(2026, 10, 1 + index)));
}

/** The month and year heading for a locale, for example `November 2026`. */
export function monthTitle(anchor: Date, locale: string): string {
  return new Intl.DateTimeFormat(locale, { month: 'long', year: 'numeric' }).format(anchor);
}
