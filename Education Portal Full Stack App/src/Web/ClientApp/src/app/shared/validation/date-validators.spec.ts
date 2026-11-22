import { FormControl, FormGroup } from '@angular/forms';
import { describe, expect, it } from 'vitest';
import {
  ACADEMIC_YEAR_MAX_DAYS,
  ageOn,
  dayNumber,
  endsAfterStart,
  endsAfterStartValidator,
  endsOnOrAfterStartValidator,
  HOLIDAY_MAX_DAYS,
  isTimeRangeValid,
  spanUnderDaysValidator,
  timeRangeValidator,
} from './date-validators';

/** A local calendar date at midnight, as the date pickers produce. */
const day = (year: number, month: number, date: number) => new Date(year, month - 1, date);

describe('dayNumber', () => {
  it('counts consecutive calendar days across a month and a leap day', () => {
    expect(dayNumber(day(2024, 2, 29)) - dayNumber(day(2024, 2, 28))).toBe(1);
    expect(dayNumber(day(2025, 1, 1)) - dayNumber(day(2024, 12, 31))).toBe(1);
  });
});

describe('ageOn (StudentProfile.AgeOn)', () => {
  it('is one year less until the birthday has come in the admission year', () => {
    expect(ageOn(day(2010, 6, 15), day(2020, 6, 14))).toBe(9);
    expect(ageOn(day(2010, 6, 15), day(2020, 6, 15))).toBe(10);
  });

  it('moves a 29 February birthday to 28 February in a year with no leap day', () => {
    expect(ageOn(day(2000, 2, 29), day(2023, 2, 27))).toBe(22);
    expect(ageOn(day(2000, 2, 29), day(2023, 2, 28))).toBe(23);
  });

  it('gives the age on the birthday itself as the years apart', () => {
    expect(ageOn(day(2015, 9, 1), day(2018, 9, 1))).toBe(3);
  });
});

describe('endsOnOrAfterStartValidator', () => {
  const rule = endsOnOrAfterStartValidator('startDate', 'endDate');
  const group = (start: Date | null, end: Date | null) =>
    new FormGroup({ startDate: new FormControl(start), endDate: new FormControl(end) });

  it('accepts an end on the start day and after it', () => {
    expect(rule(group(day(2026, 8, 1), day(2026, 8, 1)))).toBeNull();
    expect(rule(group(day(2026, 8, 1), day(2027, 6, 30)))).toBeNull();
  });

  it('refuses an end one day before the start', () => {
    expect(rule(group(day(2026, 8, 1), day(2026, 7, 31)))).toEqual({ endsBeforeStart: true });
  });

  it('leaves blank dates to the required rule', () => {
    expect(rule(group(null, day(2026, 7, 31)))).toBeNull();
  });
});

describe('spanUnderDaysValidator', () => {
  const rule = spanUnderDaysValidator('startDate', 'endDate', HOLIDAY_MAX_DAYS);
  const start = day(2026, 1, 1);
  const group = (end: Date) => new FormGroup({ startDate: new FormControl(start), endDate: new FormControl(end) });

  it('accepts a span of 119 days, the last one under the limit', () => {
    expect(rule(group(day(2026, 1, 1 + 119)))).toBeNull();
  });

  it('refuses a span of 120 days, the limit itself', () => {
    expect(rule(group(day(2026, 1, 1 + 120)))).toEqual({ spanTooLong: { maxDays: HOLIDAY_MAX_DAYS } });
  });

  it('leaves an end before the start to the end-date rule', () => {
    expect(rule(group(day(2025, 12, 1)))).toBeNull();
  });

  it('uses 731 days for an academic year: 730 days is fine, 731 is not', () => {
    const yearRule = spanUnderDaysValidator('startDate', 'endDate', ACADEMIC_YEAR_MAX_DAYS);
    expect(yearRule(group(day(2026, 1, 1 + 730)))).toBeNull();
    expect(yearRule(group(day(2026, 1, 1 + 731)))).not.toBeNull();
  });
});

describe('endsAfterStart and endsAfterStartValidator', () => {
  it('needs the end to be strictly after the start', () => {
    expect(endsAfterStart('2026-11-21T09:00', '2026-11-21T10:00')).toBe(true);
    expect(endsAfterStart('2026-11-21T10:00', '2026-11-21T10:00')).toBe(false);
    expect(endsAfterStart('2026-11-21T10:00', '2026-11-21T09:00')).toBe(false);
    expect(endsAfterStart('', '2026-11-21T09:00')).toBe(false);
  });

  it('puts endsBeforeStart on the group when the end is not after the start', () => {
    const rule = endsAfterStartValidator('startsAt', 'endsAt');
    const group = new FormGroup({
      startsAt: new FormControl('2026-11-21T10:00'),
      endsAt: new FormControl('2026-11-21T10:00'),
    });
    expect(rule(group)).toEqual({ endsBeforeStart: true });
    group.controls.endsAt.setValue('2026-11-21T11:00');
    expect(rule(group)).toBeNull();
  });
});

describe('isTimeRangeValid and timeRangeValidator', () => {
  it('needs both times and an end after the start', () => {
    expect(isTimeRangeValid('09:00', '10:00')).toBe(true);
    expect(isTimeRangeValid('10:00', '10:00')).toBe(false);
    expect(isTimeRangeValid('09:00', '')).toBe(false);
  });

  it('uses the two named controls and reports timeRange', () => {
    const rule = timeRangeValidator('startTime', 'endTime');
    const group = new FormGroup({ startTime: new FormControl('12:00'), endTime: new FormControl('11:00') });
    expect(rule(group)).toEqual({ timeRange: true });
    group.controls.endTime.setValue('13:00');
    expect(rule(group)).toBeNull();
    group.controls.endTime.setValue('');
    expect(rule(group)).toBeNull();
  });
});
