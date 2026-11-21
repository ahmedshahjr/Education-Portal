import { describe, expect, it } from 'vitest';
import { EventAudience } from '../../web-api-client';
import { audienceLabel, endsAfterStart, EVENT_AUDIENCES, fromDateInput, monthRange, toDateInput } from './school-events.logic';

describe('audienceLabel', () => {
  it('names every audience', () => {
    const labels = EVENT_AUDIENCES.map(audienceLabel);
    expect(labels.every((text) => text.length > 0)).toBe(true);
    expect(new Set(labels).size).toBe(Object.values(EventAudience).length);
  });
});

describe('monthRange', () => {
  it('runs from the first day of the month to the first day of the next', () => {
    const { from, to } = monthRange(new Date(2026, 10, 21));
    expect(toDateInput(from)).toBe('2026-11-01');
    expect(toDateInput(to)).toBe('2026-12-01');
  });

  it('rolls over the end of the year', () => {
    expect(toDateInput(monthRange(new Date(2026, 11, 31)).to)).toBe('2027-01-01');
  });
});

describe('date inputs', () => {
  it('round-trips a date', () => {
    expect(toDateInput(fromDateInput('2026-02-09')!)).toBe('2026-02-09');
  });

  it('refuses text that is not a date', () => {
    expect(fromDateInput('')).toBeNull();
    expect(fromDateInput('09/02/2026')).toBeNull();
  });
});

describe('endsAfterStart', () => {
  it('needs the end to come after the start', () => {
    expect(endsAfterStart('2026-11-21T09:00', '2026-11-21T10:00')).toBe(true);
    expect(endsAfterStart('2026-11-21T10:00', '2026-11-21T10:00')).toBe(false);
    expect(endsAfterStart('2026-11-21T10:00', '2026-11-21T09:00')).toBe(false);
  });

  it('refuses a missing time', () => {
    expect(endsAfterStart('', '2026-11-21T09:00')).toBe(false);
    expect(endsAfterStart('2026-11-21T09:00', '')).toBe(false);
  });
});
