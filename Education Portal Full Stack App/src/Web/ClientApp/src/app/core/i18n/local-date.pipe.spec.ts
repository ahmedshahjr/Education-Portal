import { describe, expect, it } from 'vitest';
import { formatLocalDate } from './local-date.pipe';

describe('formatLocalDate', () => {
  const instant = '2026-03-08T22:30:00Z'; // 9 March 03:30 in Karachi, 9 March 02:30 in Dubai

  it("shows an instant in the user's time zone, so the calendar day can differ from UTC", () => {
    expect(formatLocalDate(instant, 'en-PK', 'Asia/Karachi', 'dateTime')).toBe(
      new Intl.DateTimeFormat('en-PK', { dateStyle: 'medium', timeStyle: 'short', timeZone: 'Asia/Karachi' }).format(new Date(instant))
    );
    expect(formatLocalDate(instant, 'en', 'Asia/Karachi')).toContain('9');
    expect(formatLocalDate(instant, 'en', 'UTC')).toContain('8');
  });

  it("follows the page's culture", () => {
    expect(formatLocalDate(instant, 'ar-AE', 'Asia/Dubai')).toBe(
      new Intl.DateTimeFormat('ar-AE', { dateStyle: 'medium', timeZone: 'Asia/Dubai' }).format(new Date(instant))
    );
  });

  it('follows daylight saving, which a fixed offset cannot', () => {
    const winter = formatLocalDate('2026-01-15T12:00:00Z', 'en-GB', 'Europe/London', 'time');
    const summer = formatLocalDate('2026-07-15T12:00:00Z', 'en-GB', 'Europe/London', 'time');

    expect(winter).toBe('12:00');
    expect(summer).toBe('13:00');
  });

  it('shows nothing for a missing or invalid value', () => {
    expect(formatLocalDate(null, 'en', 'UTC')).toBe('');
    expect(formatLocalDate(undefined, 'en', 'UTC')).toBe('');
    expect(formatLocalDate('', 'en', 'UTC')).toBe('');
    expect(formatLocalDate('not a date', 'en', 'UTC')).toBe('');
  });

  it('falls back to UTC for a time zone this browser does not know', () => {
    expect(formatLocalDate(instant, 'en', 'Not/AZone', 'dateTime')).toBe(formatLocalDate(instant, 'en', 'UTC', 'dateTime'));
  });
});
