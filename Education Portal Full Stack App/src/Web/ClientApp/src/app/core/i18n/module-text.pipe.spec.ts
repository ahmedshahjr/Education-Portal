import { describe, expect, it } from 'vitest';
import { moduleText } from './module-text.pipe';

describe('moduleText', () => {
  it('translates a known module by its key', () => {
    expect(moduleText({ key: 'Grades', name: 'server name', description: 'server description' })).toBe('Grades');
    expect(moduleText({ key: 'Grades' }, 'description')).toBe('Assessments and scores per enrolment.');
  });

  it("shows the server's text for a module this app does not know yet", () => {
    expect(moduleText({ key: 'Attendance', name: 'Attendance', description: 'Daily attendance.' })).toBe('Attendance');
    expect(moduleText({ key: 'Attendance', name: 'Attendance', description: 'Daily attendance.' }, 'description')).toBe('Daily attendance.');
  });

  it('shows nothing rather than failing for an empty module', () => {
    expect(moduleText({})).toBe('');
  });
});
