import { describe, expect, it } from 'vitest';
import { formatCalendarDate } from './calendar-date.pipe';

describe('formatCalendarDate', () => {
  it('shows the calendar day it is given, in the page culture', () => {
    expect(formatCalendarDate(new Date(2026, 7, 1), 'en-US')).toBe('Aug 1, 2026');
  });

  it('shows the month and year, and the full date', () => {
    expect(formatCalendarDate(new Date(2026, 11, 31), 'en-US', 'monthYear')).toBe('December 2026');
    expect(formatCalendarDate(new Date(2026, 11, 31), 'en-US', 'full')).toBe('Thursday, December 31, 2026');
  });

  it('shows nothing for a missing or invalid date', () => {
    expect(formatCalendarDate(null, 'en-US')).toBe('');
    expect(formatCalendarDate(new Date('not a date'), 'en-US')).toBe('');
  });
});
