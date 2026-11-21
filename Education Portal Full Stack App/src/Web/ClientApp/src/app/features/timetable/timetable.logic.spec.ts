import { describe, expect, it } from 'vitest';
import { TimetableConflictKind, Weekday } from '../../web-api-client';
import { compareLessons, conflictLabel, WEEKDAYS, weekdayLabel } from './timetable.logic';

describe('timetable labels', () => {
  it('names every day and every kind of clash', () => {
    expect(WEEKDAYS.map(weekdayLabel).every((text) => text.length > 0)).toBe(true);
    expect(new Set(WEEKDAYS.map(weekdayLabel)).size).toBe(7);
    expect(Object.values(TimetableConflictKind).map(conflictLabel).every((text) => text.length > 0)).toBe(true);
  });
});

describe('compareLessons', () => {
  it('orders by the days of the school week, then by start time', () => {
    const lessons = [
      { day: Weekday.Tuesday, start: '09:00:00' },
      { day: Weekday.Monday, start: '11:00:00' },
      { day: Weekday.Monday, start: '08:30:00' },
      { day: Weekday.Sunday, start: '08:00:00' },
    ];

    expect([...lessons].sort(compareLessons)).toEqual([
      { day: Weekday.Monday, start: '08:30:00' },
      { day: Weekday.Monday, start: '11:00:00' },
      { day: Weekday.Tuesday, start: '09:00:00' },
      { day: Weekday.Sunday, start: '08:00:00' },
    ]);
  });
});
