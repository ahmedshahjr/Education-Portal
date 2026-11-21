import { TimetableConflictKind, Weekday } from '../../web-api-client';

/** The days a lesson can be on, in the order of a school week. */
export const WEEKDAYS: readonly Weekday[] = [
  Weekday.Monday,
  Weekday.Tuesday,
  Weekday.Wednesday,
  Weekday.Thursday,
  Weekday.Friday,
  Weekday.Saturday,
  Weekday.Sunday,
];

export const ROOM_MAX_LENGTH = 50;

/** The name of a day, in the page's language. */
export function weekdayLabel(day: Weekday): string {
  switch (day) {
    case Weekday.Monday:
      return $localize`:@@timetable.dayMonday:Monday`;
    case Weekday.Tuesday:
      return $localize`:@@timetable.dayTuesday:Tuesday`;
    case Weekday.Wednesday:
      return $localize`:@@timetable.dayWednesday:Wednesday`;
    case Weekday.Thursday:
      return $localize`:@@timetable.dayThursday:Thursday`;
    case Weekday.Friday:
      return $localize`:@@timetable.dayFriday:Friday`;
    case Weekday.Saturday:
      return $localize`:@@timetable.daySaturday:Saturday`;
    case Weekday.Sunday:
      return $localize`:@@timetable.daySunday:Sunday`;
  }
}

/** Why a lesson clashes with another one, in the page's language. */
export function conflictLabel(kind: TimetableConflictKind): string {
  switch (kind) {
    case TimetableConflictKind.Teacher:
      return $localize`:@@timetable.conflictTeacher:The teacher already has a lesson at this time.`;
    case TimetableConflictKind.Room:
      return $localize`:@@timetable.conflictRoom:The room is already booked at this time.`;
    case TimetableConflictKind.ClassOffering:
      return $localize`:@@timetable.conflictClass:The class already has a lesson at this time.`;
  }
}

/** Orders lessons by day of the school week, then by start time ("HH:mm" or "HH:mm:ss" sort as text). */
export function compareLessons(
  a: { day: Weekday; start: string },
  b: { day: Weekday; start: string },
): number {
  return WEEKDAYS.indexOf(a.day) - WEEKDAYS.indexOf(b.day) || a.start.localeCompare(b.start);
}
