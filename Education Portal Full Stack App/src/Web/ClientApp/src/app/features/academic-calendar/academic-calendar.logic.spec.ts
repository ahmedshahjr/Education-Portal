import { describe, expect, it } from 'vitest';
import { coversDay, dateKey, fromApiDate, fromApiTime, isTimeRangeValid, monthRange, toApiTime } from './academic-calendar.logic';

describe('academic calendar dates', () => {
  it('keeps the calendar day the API sent, whatever the time zone', () => {
    const local = fromApiDate(new Date('2026-08-01'));
    expect(dateKey(local)).toBe('2026-08-01');
  });

  it('writes a day as yyyy-MM-dd with leading zeros', () => {
    expect(dateKey(new Date(2026, 0, 5))).toBe('2026-01-05');
  });

  it('gives the first and last day of a month, including February in a leap year', () => {
    const { from, to } = monthRange(new Date(2028, 1, 14));
    expect(dateKey(from)).toBe('2028-02-01');
    expect(dateKey(to)).toBe('2028-02-29');
  });

  it('includes both ends of a range and nothing outside it', () => {
    const start = new Date(2026, 11, 24);
    const end = new Date(2026, 11, 26);
    expect(coversDay(start, end, new Date(2026, 11, 24))).toBe(true);
    expect(coversDay(start, end, new Date(2026, 11, 26))).toBe(true);
    expect(coversDay(start, end, new Date(2026, 11, 23))).toBe(false);
    expect(coversDay(start, end, new Date(2026, 11, 27))).toBe(false);
  });
});

describe('academic calendar times', () => {
  it('adds seconds to a time input value and leaves a blank one out', () => {
    expect(toApiTime('08:30')).toBe('08:30:00');
    expect(toApiTime('08:30:15')).toBe('08:30:15');
    expect(toApiTime('  ')).toBeUndefined();
    expect(toApiTime(undefined)).toBeUndefined();
  });

  it('shows an API time without seconds', () => {
    expect(fromApiTime('13:05:00')).toBe('13:05');
    expect(fromApiTime(undefined)).toBe('');
  });

  it('accepts a range only when the end is after the start', () => {
    expect(isTimeRangeValid('08:00', '13:00')).toBe(true);
    expect(isTimeRangeValid('08:00', '08:00')).toBe(false);
    expect(isTimeRangeValid('13:00', '08:00')).toBe(false);
    expect(isTimeRangeValid('', '08:00')).toBe(false);
  });
});
