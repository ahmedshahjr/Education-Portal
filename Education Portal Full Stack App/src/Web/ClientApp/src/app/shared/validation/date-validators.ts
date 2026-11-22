import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';

const MILLISECONDS_PER_DAY = 86_400_000;

/**
 * The number of the calendar day a local date falls on, counted from 1970-01-01. Two dates compare by days, not by
 * time of day or time zone, the same way the server compares its `DateOnly` values.
 */
export function dayNumber(day: Date): number {
  return Math.round(Date.UTC(day.getFullYear(), day.getMonth(), day.getDate()) / MILLISECONDS_PER_DAY);
}

function isLeapYear(year: number): boolean {
  return (year % 4 === 0 && year % 100 !== 0) || year % 400 === 0;
}

/**
 * The age in whole years on a day (StudentProfile.AgeOn): the years between the two dates, less one when the birthday has not
 * come yet in the year of `on`.
 */
export function ageOn(dateOfBirth: Date, on: Date): number {
  let age = on.getFullYear() - dateOfBirth.getFullYear();
  // DateOnly.AddYears moves a 29 February birthday to 28 February in a year that has none.
  const leapBirthday = dateOfBirth.getMonth() === 1 && dateOfBirth.getDate() === 29 && !isLeapYear(on.getFullYear());
  const birthdayThisYear = leapBirthday
    ? new Date(on.getFullYear(), 1, 28)
    : new Date(on.getFullYear(), dateOfBirth.getMonth(), dateOfBirth.getDate());
  if (dayNumber(on) < dayNumber(birthdayThisYear)) {
    age--;
  }
  return age;
}

/** The end date must not be before the start date (the server's DateRangeInvalid). Blank dates are left to `required`. */
export function endsOnOrAfterStartValidator(startKey: string, endKey: string): ValidatorFn {
  return (group: AbstractControl): ValidationErrors | null => {
    const start = group.get(startKey)?.value as Date | null | undefined;
    const end = group.get(endKey)?.value as Date | null | undefined;
    return start && end && dayNumber(end) < dayNumber(start) ? { endsBeforeStart: true } : null;
  };
}

/**
 * The span from start to end must be fewer than `maxDays` days (the server's MaxLengthInDays: a year is under 731 days, a
 * holiday under 120). Only checked once the end is on or after the start.
 */
export function spanUnderDaysValidator(startKey: string, endKey: string, maxDays: number): ValidatorFn {
  return (group: AbstractControl): ValidationErrors | null => {
    const start = group.get(startKey)?.value as Date | null | undefined;
    const end = group.get(endKey)?.value as Date | null | undefined;
    if (!start || !end || dayNumber(end) < dayNumber(start)) {
      return null;
    }
    return dayNumber(end) - dayNumber(start) < maxDays ? null : { spanTooLong: { maxDays } };
  };
}

/** An event's end time must be after its start time, both as datetime-local text (`yyyy-MM-ddTHH:mm`). */
export function endsAfterStartValidator(startKey: string, endKey: string): ValidatorFn {
  return (group: AbstractControl): ValidationErrors | null => {
    const startsAt = group.get(startKey)?.value as string;
    const endsAt = group.get(endKey)?.value as string;
    return startsAt && endsAt && !endsAfterStart(startsAt, endsAt) ? { endsBeforeStart: true } : null;
  };
}

/** True when an event ends after it starts. Both are the values of datetime-local inputs. */
export function endsAfterStart(startsAt: string, endsAt: string): boolean {
  const start = new Date(startsAt).getTime();
  const end = new Date(endsAt).getTime();
  return !Number.isNaN(start) && !Number.isNaN(end) && end > start;
}

/** True when a time range is complete and ends after it starts (`HH:mm` strings compare in order). */
export function isTimeRangeValid(start: string, end: string): boolean {
  return start !== '' && end !== '' && end > start;
}

/** The end time must come after the start time (the server's TimeRangeInvalid). Keys name the two time controls. */
export function timeRangeValidator(startKey: string, endKey: string): ValidatorFn {
  return (group: AbstractControl): ValidationErrors | null => {
    const start = group.get(startKey)?.value as string;
    const end = group.get(endKey)?.value as string;
    return start && end && !isTimeRangeValid(start, end) ? { timeRange: true } : null;
  };
}

/** The most days an academic year may span: the span must be under 731 days (AcademicYear.MaxLengthInDays). */
export const ACADEMIC_YEAR_MAX_DAYS = 731;

/** The most days a holiday may span: the span must be under 120 days (Holiday.MaxLengthInDays). */
export const HOLIDAY_MAX_DAYS = 120;
