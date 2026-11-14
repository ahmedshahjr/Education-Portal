import { describe, expect, it } from 'vitest';
import { moduleText } from './module-text.pipe';

describe('moduleText', () => {
  it('translates a known module by its code', () => {
    expect(moduleText({ code: 'EXAMS', name: 'server name', description: 'server description' })).toBe('Exams');
    expect(moduleText({ code: 'EXAMS' }, 'description')).toBe('Assessments, scores and report cards.');
  });

  it("shows the server's text for a module this app does not know yet", () => {
    expect(moduleText({ code: 'CANTEEN', name: 'Canteen', description: 'Meals.' })).toBe('Canteen');
    expect(moduleText({ code: 'CANTEEN', name: 'Canteen', description: 'Meals.' }, 'description')).toBe('Meals.');
  });

  it('shows nothing rather than failing for an empty module', () => {
    expect(moduleText({})).toBe('');
  });
});
